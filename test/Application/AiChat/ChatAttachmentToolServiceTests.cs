using System.Text.Json;
using Application.Abstractions.AiChat;
using Application.Configuration.AI;
using Application.Services.AiChat;
using Application.Services.Transactions;
using Domain.Entities.Card;
using Domain.Entities.Configuration;
using Infrastructure.AI.Chat;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.AI;
using Shouldly;
using Tests.Application.Card.CardPurchases;
using DomainTransactions = global::Domain.Entities.Transactions;

namespace Tests.Application.AiChat;

public class ChatAttachmentToolServiceTests : CardFlowTestBase
{
    private readonly InMemoryChatAttachmentStore _store;
    private readonly ChatAttachmentContext _context = new();
    private readonly ChatActionCollector _actions = new();
    private readonly List<DomainTransactions.Transaction> _existing = new();

    public ChatAttachmentToolServiceTests()
    {
        var monitor = new Mock<IOptionsMonitor<AiSettings>>();
        monitor.Setup(m => m.CurrentValue).Returns(new AiSettings { Chat = new AiChatSettings() });
        _store = new InMemoryChatAttachmentStore(monitor.Object);

        CreditCards.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<CreditCard> { new(10, 1, 1, "Nubank Vitor", 28, 20, 5000m) });
        Transactions.Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(_existing);
        Categories.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<Category> { Category(3, "E"), Category(9, "E") });
        Categories.Setup(c => c.GetCardInvoiceCategoryAsync(It.IsAny<int>())).ReturnsAsync(Category(9, "E"));
    }

    private ChatAttachmentToolService Build()
    {
        var fuzzy = new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build());
        var memory = new TransactionCategoryMemoryBuilder(Transactions.Object, Purchases.Object, CategoryItems.Object, fuzzy);
        var classification = new ImportedTransactionClassificationService(
            memory, Classifier.Object, NullLogger<ImportedTransactionClassificationService>.Instance);

        var analyzer = new ChatAttachmentAnalyzer(
            Transactions.Object, CreditCards.Object, new CardInvoicePaymentDetector(), new AttachmentDuplicateDetector());

        return new ChatAttachmentToolService(
            _store, _context, analyzer, Categories.Object, new TransactionDraftCategorizer(classification), _actions, CurrentUser.Object);
    }

    private string Attach(string id, params (string date, string description, decimal amount, string type)[] rows)
    {
        _store.Add(1, new ChatAttachment(
            id,
            $"{id}.pdf",
            rows.Select(r => new AttachmentTransaction(DateTime.Parse(r.date), r.description, r.amount, r.type)).ToList(),
            DateTime.UtcNow));

        _context.Set(_context.Ids.Append(id));
        return id;
    }

    private static string Json(object? data) => JsonSerializer.Serialize(data);

    private void NothingWasWritten()
    {
        Transactions.Verify(t => t.AddAsync(It.IsAny<DomainTransactions.Transaction>()), Times.Never);
        Transactions.Verify(t => t.AddRangeAsync(It.IsAny<IEnumerable<DomainTransactions.Transaction>>()), Times.Never);
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    private void Standard() => Attach("extrato",
        ("2026-06-05", "Mercado", 100m, "E"),
        ("2026-09-09", "Uber", 20m, "E"),
        ("2026-09-10", "Padaria", 15m, "E"),
        ("2026-09-15", "PAGAMENTO FATURA NUBANK", 900m, "E"),
        ("2026-09-20", "Salario", 3000m, "I"));

    [Fact]
    public async Task Sem_anexo_ou_expirado_pede_para_reenviar()
    {
        var result = await Build().PrepareTransactionsAsync(null, null, null, null, null, null, false, true);

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("attach");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Anexo_inexistente_lista_os_disponiveis()
    {
        Standard();

        var result = await Build().SummarizeAsync("outro");

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("extrato");
    }

    [Fact]
    public async Task A_partir_de_uma_data_abre_o_modal_so_com_as_linhas_do_periodo_ordenadas()
    {
        Standard();

        var result = await Build().PrepareTransactionsAsync(null, "2026-09-10", null, null, null, null, false, true);

        result.Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenTransactionsModal);
        action.TransactionDrafts!.Select(d => d.Description).ShouldBe(new[] { "Padaria", "PAGAMENTO FATURA NUBANK", "Salario" });
        action.TransactionDrafts!.Select(d => d.Date).ShouldBe(new[] { "2026-09-10", "2026-09-15", "2026-09-20" });
        NothingWasWritten();
    }

    [Fact]
    public async Task Pagamento_de_fatura_vai_com_a_categoria_de_fatura_e_pode_ser_excluido()
    {
        Standard();

        await Build().PrepareTransactionsAsync(null, null, null, null, null, null, false, true);

        var invoice = _actions.Actions.Single().TransactionDrafts!.Single(d => d.IsCardInvoicePayment);
        invoice.CategoryId.ShouldBe(9);
        invoice.Type.ShouldBe("E");

        var summary = Json((await Build().PrepareTransactionsAsync(null, null, null, null, null, null, false, true)).Data);
        summary.ShouldContain("\"cardInvoicePayments\":1");

        var other = new ChatActionCollector();
        var withoutInvoices = await BuildWith(other).PrepareTransactionsAsync(null, null, null, null, null, null, false, false);

        other.Actions.Single().TransactionDrafts!.ShouldNotContain(d => d.IsCardInvoicePayment);
        withoutInvoices.Ok.ShouldBeTrue();
    }

    private ChatAttachmentToolService BuildWith(ChatActionCollector actions)
    {
        var fuzzy = new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build());
        var memory = new TransactionCategoryMemoryBuilder(Transactions.Object, Purchases.Object, CategoryItems.Object, fuzzy);
        var classification = new ImportedTransactionClassificationService(
            memory, Classifier.Object, NullLogger<ImportedTransactionClassificationService>.Instance);

        var analyzer = new ChatAttachmentAnalyzer(
            Transactions.Object, CreditCards.Object, new CardInvoicePaymentDetector(), new AttachmentDuplicateDetector());

        return new ChatAttachmentToolService(
            _store, _context, analyzer, Categories.Object, new TransactionDraftCategorizer(classification), actions, CurrentUser.Object);
    }

    [Fact]
    public async Task Repetidas_vem_marcadas_e_com_includeDuplicates_perdem_a_marca()
    {
        Standard();
        _existing.Add(new DomainTransactions.Transaction(
            userId: 1, amount: 15m, transactionDate: new DateTime(2026, 9, 10), description: "Padaria",
            codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P"));

        await Build().PrepareTransactionsAsync(null, "2026-09-01", null, null, null, null, false, true);

        _actions.Actions.Single().TransactionDrafts!.Single(d => d.Description == "Padaria").DuplicateKind.ShouldBe(DuplicateKinds.AlreadyRegistered);

        var included = new ChatActionCollector();
        await BuildWith(included).PrepareTransactionsAsync(null, "2026-09-01", null, null, null, null, true, true);

        included.Actions.Single().TransactionDrafts!.ShouldAllBe(d => d.DuplicateKind == null);
    }

    [Fact]
    public async Task Filtros_de_texto_tipo_e_exclusao_funcionam_e_nada_encontrado_pergunta()
    {
        Standard();

        await Build().PrepareTransactionsAsync(null, null, null, "income", null, null, false, true);
        _actions.Actions.Single().TransactionDrafts!.Select(d => d.Description).ShouldBe(new[] { "Salario" });

        var none = await Build().PrepareTransactionsAsync(null, null, null, null, "xyz", null, false, true);
        none.Ok.ShouldBeFalse();

        var bad = await Build().PrepareTransactionsAsync(null, "10/09", null, null, null, null, false, true);
        bad.Ok.ShouldBeFalse();
        bad.Message.ShouldContain("yyyy-MM-dd");
    }

    [Fact]
    public async Task Resumo_separa_pagamento_de_fatura_dos_gastos()
    {
        Standard();

        var json = Json((await Build().SummarizeAsync(null)).Data);

        json.ShouldContain("\"TotalExpense\":135");
        json.ShouldContain("\"CardInvoicePaymentCount\":1");
        json.ShouldContain("\"CardInvoicePaymentTotal\":900");
        json.ShouldContain("\"TotalIncome\":3000");
    }

    [Fact]
    public async Task Verifica_repetidas_e_lista_as_transacoes_filtradas()
    {
        Standard();
        _existing.Add(new DomainTransactions.Transaction(
            userId: 1, amount: 20m, transactionDate: new DateTime(2026, 9, 9), description: "Uber",
            codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P"));

        var duplicates = Json((await Build().CheckDuplicatesAsync(null, null, null)).Data);
        duplicates.ShouldContain("\"alreadyRegistered\":1");

        var listed = Json((await Build().GetTransactionsAsync(null, null, null, null, null, "card_invoice_payment", 20)).Data);
        listed.ShouldContain("\"totalMatches\":1");
        listed.ShouldContain("Nubank Vitor");
    }

    [Fact]
    public async Task Descreve_o_anexo_para_o_prompt_com_periodo_e_totais()
    {
        Standard();

        var summary = (await Build().DescribeAsync(1, new[] { "extrato", "inexistente" })).Single();

        summary.TransactionCount.ShouldBe(5);
        summary.PeriodStart.ShouldBe("2026-06-05");
        summary.PeriodEnd.ShouldBe("2026-09-20");
        summary.TotalExpense.ShouldBe(135m);
        summary.CardInvoicePaymentCount.ShouldBe(1);
    }
}
