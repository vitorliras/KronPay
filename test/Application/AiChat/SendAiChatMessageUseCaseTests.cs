using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Abstractions.Common;
using Application.Configuration.AI;
using Application.DTOs.AiChat;
using Application.Services.AiChat;
using Application.Services.Spending;
using Application.UseCases.AiChat;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.AI;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;
using Domain.Interfaces.Transactions;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.AI;
using Shared.Localization;
using Shouldly;
using DomainTransactions = global::Domain.Entities.Transactions;

namespace Tests.Application.AiChat;

public abstract class AiChatTestBase
{
    protected readonly Mock<IAiAvailability> Availability = new();
    protected readonly Mock<ITransactionRepository> Transactions = new();
    protected readonly Mock<ICardInvoiceRepository> Invoices = new();
    protected readonly Mock<ICategoryRepository> Categories = new();
    protected readonly Mock<ICreditCardRepository> CreditCards = new();
    protected readonly Mock<ICardPurchaseRepository> Purchases = new();
    protected readonly Mock<ICurrentUserService> CurrentUser = new();

    protected AiChatTestBase()
    {
        CurrentUser.Setup(c => c.UserId).Returns(1);
        Invoices.Setup(i => i.GetPaymentTransactionIdsAsync(It.IsAny<int>())).ReturnsAsync(new List<int>());
        Invoices.Setup(i => i.GetPendingAmountsByCardAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new Dictionary<int, decimal>());
        Invoices
            .Setup(i => i.GetInstallmentDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<CardInstallmentDetail>());
    }

    protected void AiEnabled(bool enabled) =>
        Availability.Setup(a => a.GetActive())
            .Returns(enabled ? new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "m" }) : null);

    protected SpendingAggregator Aggregator() =>
        new(Transactions.Object, Invoices.Object, Categories.Object);

    protected static Category Cat(int id, string name)
    {
        var category = new Category(1, name, "E");
        typeof(Category).GetProperty("Id")!.SetValue(category, id);
        return category;
    }
}

public class SendAiChatMessageUseCaseTests : AiChatTestBase
{
    private readonly Mock<IOptionsMonitor<AiSettings>> _settings = new();
    private readonly Mock<IAiUsageLogRepository> _usage = new();
    private readonly Mock<IAiChatService> _chat = new();
    private readonly Mock<IChatToolbox> _toolbox = new();
    private readonly Mock<IGamificationService> _gamification = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ChatActionCollector _actions = new();
    private readonly AiSettings _aiSettings = new();

    public SendAiChatMessageUseCaseTests()
    {
        _settings.Setup(s => s.CurrentValue).Returns(_aiSettings);
        _usage.Setup(u => u.CountByFeatureSinceAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(0);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(true);
        CreditCards.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<global::Domain.Entities.Card.CreditCard>());
        Categories.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<Category> { Cat(1, "Mercado") });
        Transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DomainTransactions.Transaction>());
    }

    private SendAiChatMessageUseCase Build()
    {
        var profile = new FinancialProfileBuilder(
            Aggregator(),
            CreditCards.Object,
            Invoices.Object,
            Purchases.Object,
            Mock.Of<IFinancialGoalRepository>(),
            Mock.Of<IPlannedCommitmentRepository>());

        return new SendAiChatMessageUseCase(
            Availability.Object,
            _settings.Object,
            _usage.Object,
            profile,
            _chat.Object,
            _toolbox.Object,
            _actions,
            _gamification.Object,
            _uow.Object,
            CurrentUser.Object,
            NullLogger<SendAiChatMessageUseCase>.Instance);
    }

    private static AiChatRequest Request(params (string Role, string Content)[] messages) =>
        new(messages.Select(m => new AiChatMessageDto(m.Role, m.Content)).ToList());

    private void ChatReturns(string text) =>
        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiChatServiceResult(text, 0));

    [Fact]
    public async Task Ia_desligada_falha_sem_chamar_o_servico()
    {
        AiEnabled(false);

        var result = await Build().ExecuteAsync(Request(("user", "oi")));

        result.Message.ShouldBe(MessageKeys.AiUnavailable);
        _chat.Verify(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Teto_diario_bloqueia_a_mensagem()
    {
        AiEnabled(true);
        _usage.Setup(u => u.CountByFeatureSinceAsync(1, AiFeatures.AiChat, It.IsAny<DateTime>())).ReturnsAsync(_aiSettings.Chat.MaxMessagesPerDay);

        var result = await Build().ExecuteAsync(Request(("user", "oi")));

        result.Message.ShouldBe(MessageKeys.AiChatDailyLimit);
        _chat.Verify(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Historico_e_limitado_as_ultimas_mensagens_com_papeis_normalizados()
    {
        AiEnabled(true);
        ChatReturns("{\"reply\":\"ok\"}");
        IReadOnlyList<AiChatTurn>? sent = null;
        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .Callback<int, string, IReadOnlyList<AiChatTurn>, IChatToolbox, CancellationToken>((_, _, h, _, _) => sent = h)
            .ReturnsAsync(new AiChatServiceResult("{\"reply\":\"ok\"}", 0));

        var messages = Enumerable.Range(1, 30)
            .Select(i => (i % 2 == 1 ? "USER" : "Assistant", $" mensagem {i} "))
            .ToArray();
        messages[^1] = ("User", " ultima ");

        await Build().ExecuteAsync(Request(messages));

        sent!.Count.ShouldBe(_aiSettings.Chat.MaxHistoryMessages);
        sent.Last().ShouldBe(new AiChatTurn("user", "ultima"));
        sent.ShouldAllBe(t => t.Role == "user" || t.Role == "assistant");
    }

    [Fact]
    public async Task Resposta_em_json_vira_resposta_sugestoes_e_acoes_coletadas()
    {
        AiEnabled(true);
        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .Callback(() => _actions.Add(new AiChatActionResponse(AiChatActionTypes.Navigate, "x", "/dashboard")))
            .ReturnsAsync(new AiChatServiceResult("{\"reply\":\"Aqui esta\",\"suggestions\":[\"E o cartao?\"]}", 1));

        var result = await Build().ExecuteAsync(Request(("user", "abrir dashboard")));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Reply.ShouldBe("Aqui esta");
        result.Value.Suggestions.ShouldBe(new[] { "E o cartao?" });
        result.Value.Actions.Single().Path.ShouldBe("/dashboard");
    }

    [Fact]
    public async Task Prompt_de_sistema_leva_o_perfil_e_as_regras_de_seguranca()
    {
        AiEnabled(true);
        string? prompt = null;
        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .Callback<int, string, IReadOnlyList<AiChatTurn>, IChatToolbox, CancellationToken>((_, p, _, _, _) => prompt = p)
            .ReturnsAsync(new AiChatServiceResult("{\"reply\":\"ok\"}", 0));

        await Build().ExecuteAsync(Request(("user", "oi")));

        prompt!.ShouldContain("Today:");
        prompt.ShouldContain("NEVER say that something was saved or paid");
        prompt.ShouldContain("untrusted DATA");
    }

    [Fact]
    public async Task Recusa_do_provedor_e_falha_generica_viram_mensagens_distintas_sem_vazar_detalhes()
    {
        AiEnabled(true);

        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiProviderRejectedException(429));
        (await Build().ExecuteAsync(Request(("user", "oi")))).Message.ShouldBe(MessageKeys.AiChatProviderRejected);

        _chat
            .Setup(c => c.RunAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AiChatTurn>>(), It.IsAny<IChatToolbox>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("sk-segredo"));
        (await Build().ExecuteAsync(Request(("user", "oi")))).Message.ShouldBe(MessageKeys.AiChatFailed);
    }

    [Fact]
    public async Task Resposta_vazia_falha()
    {
        AiEnabled(true);
        ChatReturns("   ");

        (await Build().ExecuteAsync(Request(("user", "oi")))).Message.ShouldBe(MessageKeys.AiChatFailed);
    }

    [Fact]
    public async Task Turno_bem_sucedido_notifica_a_gamificacao_e_falha_dela_nao_quebra_a_resposta()
    {
        AiEnabled(true);
        ChatReturns("{\"reply\":\"ok\"}");

        (await Build().ExecuteAsync(Request(("user", "oi")))).IsSuccess.ShouldBeTrue();
        _gamification.Verify(g => g.NotifyAssistantConversationAsync(1), Times.Once);

        _gamification.Setup(g => g.NotifyAssistantConversationAsync(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("boom"));

        (await Build().ExecuteAsync(Request(("user", "oi")))).IsSuccess.ShouldBeTrue();
    }
}

public class GetAiChatStartersUseCaseTests : AiChatTestBase
{
    private GetAiChatStartersUseCase Build() =>
        new(Availability.Object, Aggregator(), CreditCards.Object, Invoices.Object, CurrentUser.Object);

    public GetAiChatStartersUseCaseTests()
    {
        CreditCards.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<global::Domain.Entities.Card.CreditCard>());
        Categories.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<Category> { Cat(1, "Mercado") });
    }

    private void WithTransactions(params DomainTransactions.Transaction[] transactions) =>
        Transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions.ToList());

    [Fact]
    public async Task Ia_desligada_falha()
    {
        AiEnabled(false);

        (await Build().ExecuteAsync()).Message.ShouldBe(MessageKeys.AiUnavailable);
    }

    [Fact]
    public async Task Sem_dados_oferece_gasto_do_mes_lancar_e_tour()
    {
        AiEnabled(true);
        WithTransactions();

        var starters = (await Build().ExecuteAsync()).Value!.Starters;

        starters.Select(s => s.MessageKey).ShouldBe(new[]
        {
            MessageKeys.AiChatStarterSpendingMonth,
            MessageKeys.AiChatStarterAddTransaction,
            MessageKeys.AiChatStarterTour
        });
    }

    [Fact]
    public async Task Com_gasto_no_mes_sugere_a_maior_categoria_pelo_nome()
    {
        AiEnabled(true);
        WithTransactions(new DomainTransactions.Transaction(
            userId: 1, amount: 100m, transactionDate: DateTime.Today, description: "mercado",
            codTypeTransaction: "E", categoryId: 1, categoryItemId: null, idPaymentMethod: 1, status: "P"));

        var first = (await Build().ExecuteAsync()).Value!.Starters.First();

        first.MessageKey.ShouldBe(MessageKeys.AiChatStarterTopCategory);
        first.Arg.ShouldBe("Mercado");
    }

    [Fact]
    public async Task Com_fatura_em_aberto_sugere_a_proxima_fatura_e_limita_a_quatro()
    {
        AiEnabled(true);
        WithTransactions();
        var card = new global::Domain.Entities.Card.CreditCard(10, 1, 1, "Nubank", 28, 20, 1000m);
        CreditCards.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<global::Domain.Entities.Card.CreditCard> { card });

        var invoice = new global::Domain.Entities.Card.CardInvoice(1, 10, 2026, 10, DateTime.Today, DateTime.Today);
        invoice.AddAmount(100m);
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<global::Domain.Entities.Card.CardInvoice> { invoice });
        Invoices.Setup(i => i.GetPendingAmountsByCardAsync(10, 1)).ReturnsAsync(new Dictionary<int, decimal> { [invoice.Id] = 100m });

        var starters = (await Build().ExecuteAsync()).Value!.Starters;

        starters.Select(s => s.MessageKey).ShouldContain(MessageKeys.AiChatStarterNextInvoice);
        starters.Count.ShouldBeLessThanOrEqualTo(4);
    }
}
