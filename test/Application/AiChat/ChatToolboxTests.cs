using System.Text.Json;
using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Services.AiChat;
using Application.Services.Assistant;
using Application.Services.Spending;
using Application.Services.Transactions;
using Domain.Entities.Card;
using Domain.Entities.Configuration;
using Domain.Entities.Goals;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Goals;
using Domain.Interfaces.Planning;
using Domain.Services.Transactions;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.AI;
using Shouldly;
using Tests.Application.Card.CardPurchases;
using DomainTransactions = global::Domain.Entities.Transactions;

namespace Tests.Application.AiChat;

public class ChatToolboxTests : CardFlowTestBase
{
    private readonly Mock<IPaymentMethodRepository> _methods = new();
    private readonly Mock<IFinancialGoalRepository> _goals = new();
    private readonly Mock<IPlannedCommitmentRepository> _commitments = new();
    private ChatActionCollector _actions = new();
    private readonly Dictionary<int, decimal> _pending = new();

    public ChatToolboxTests()
    {
        CreditCards.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<CreditCard>
        {
            new(10, 1, 1, "Nubank Vitor", 28, 20, 5000m),
            new(11, 1, 1, "Itau Lany", 28, 20, 3000m)
        });

        CreditCards.Setup(c => c.GetByIdAsync(11, 1)).ReturnsAsync(new CreditCard(11, 1, 1, "Itau Lany", 28, 20, 3000m));

        Invoices
            .Setup(i => i.GetInstallmentDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<CardInstallmentDetail>());
        Invoices.Setup(i => i.GetByCardAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<CardInvoice>());
        Invoices.Setup(i => i.GetPendingAmountsByCardAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(_pending);

        _methods.Setup(m => m.GetAllAsync(1)).ReturnsAsync(new List<PaymentMethod>
        {
            Method(1, "Pix"),
            Method(2, "Debito")
        });

        _goals.Setup(g => g.GetActiveAsync(1)).ReturnsAsync(new List<FinancialGoal>());
        _commitments.Setup(c => c.GetByUserAsync(1)).ReturnsAsync(new List<global::Domain.Entities.Planning.PlannedCommitment>());
        Categories.Setup(c => c.GetCardInvoiceCategoryAsync(It.IsAny<int>())).ReturnsAsync(Category(9, "E"));
        Categories.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<Category>
        {
            Named(3, "Mercado", "E"),
            Named(6, "Transporte", "E"),
            Named(5, "Salario", "I")
        });
        CategoryItems.Setup(i => i.GetAllAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<CategoryItem>());
    }

    private static PaymentMethod Method(int id, string description)
    {
        var method = new PaymentMethod(1, description);
        typeof(PaymentMethod).GetProperty("Id")!.SetValue(method, id);
        return method;
    }

    private static CardInvoice Invoice(int id, int cardId, short year, short month, decimal amount, bool paid = false)
    {
        var invoice = new CardInvoice(1, cardId, year, month, new DateTime(year, month, 20), new DateTime(year, month, 28));
        typeof(CardInvoice).GetProperty("Id")!.SetValue(invoice, id);

        if (amount > 0)
            invoice.AddAmount(amount);

        if (paid)
        {
            invoice.Pay(new DomainTransactions.Transaction(
                userId: 1, amount: 10m, transactionDate: DateTime.Today, description: "p",
                codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1));
        }

        return invoice;
    }

    private void Pending(int invoiceId, decimal amount) => _pending[invoiceId] = amount;

    private IChatToolbox Build()
    {
        var aggregator = new SpendingAggregator(Transactions.Object, Invoices.Object, Categories.Object);
        var fuzzy = new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build());

        var memory = new TransactionCategoryMemoryBuilder(Transactions.Object, Purchases.Object, CategoryItems.Object, fuzzy);
        var classification = new ImportedTransactionClassificationService(
            memory, Classifier.Object, NullLogger<ImportedTransactionClassificationService>.Instance);

        return new ChatToolbox(
            aggregator, Transactions.Object, CreditCards.Object, Invoices.Object, Purchases.Object,
            _methods.Object, _goals.Object, _commitments.Object, fuzzy, new AssistantTree(), _actions,
            classification, PreviewBuilder(), Categories.Object, CategoryItems.Object, CurrentUser.Object);
    }

    private static Category Named(int id, string name, string type)
    {
        var category = new Category(1, name, type);
        typeof(Category).GetProperty("Id")!.SetValue(category, id);
        return category;
    }

    private static CategoryItem Item(int categoryId, string description)
    {
        var item = new CategoryItem(categoryId, description);
        typeof(CategoryItem).GetProperty("Id")!.SetValue(item, 1);
        return item;
    }

    private static TransactionToolItem Tx(
        string? description = "almoco", decimal? amount = 50m, string? date = null,
        string? type = null, string? method = null, string? category = null) =>
        new(description, amount, date, type, method, category);

    private static string Json(object? data) => JsonSerializer.Serialize(data);

    private void NothingWasWritten()
    {
        Invoices.Verify(i => i.Update(It.IsAny<CardInvoice>()), Times.Never);
        Invoices.Verify(i => i.AddAsync(It.IsAny<CardInvoice>()), Times.Never);
        Purchases.Verify(p => p.AddAsync(It.IsAny<CardPurchase>()), Times.Never);
        Transactions.Verify(t => t.AddAsync(It.IsAny<DomainTransactions.Transaction>()), Times.Never);
        Transactions.Verify(t => t.AddRangeAsync(It.IsAny<IEnumerable<DomainTransactions.Transaction>>()), Times.Never);
        Categories.Verify(c => c.AddAsync(It.IsAny<Category>()), Times.Never);
        CategoryItems.Verify(i => i.AddAsync(It.IsAny<CategoryItem>()), Times.Never);
        _methods.Verify(m => m.AddAsync(It.IsAny<PaymentMethod>()), Times.Never);
        _goals.Verify(g => g.AddAsync(It.IsAny<FinancialGoal>()), Times.Never);
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    [Fact]
    public async Task Gastos_por_categoria_rejeita_data_invalida_e_intervalo_gigante()
    {
        var toolbox = Build();

        (await toolbox.GetSpendingByCategory("ontem", "2026-10-10")).Ok.ShouldBeFalse();
        (await toolbox.GetSpendingByCategory("2026-10-10", "2026-10-01")).Ok.ShouldBeFalse();
        (await toolbox.GetSpendingByCategory("2020-01-01", "2026-10-01")).Ok.ShouldBeFalse();
    }

    [Fact]
    public async Task Gastos_por_categoria_avisa_quando_nao_ha_categoria_de_fatura()
    {
        Categories.Setup(c => c.GetCardInvoiceCategoryAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        var result = await Build().GetSpendingByCategory("2026-10-01", "2026-10-31");

        result.Ok.ShouldBeTrue();
        Json(result.Data).ShouldContain("WARNING");
    }

    [Fact]
    public async Task Busca_limita_a_30_linhas_mesmo_pedindo_mais()
    {
        var many = Enumerable.Range(1, 45)
            .Select(i => new DomainTransactions.Transaction(
                userId: 1, amount: i, transactionDate: DateTime.Today.AddDays(-1), description: $"compra {i}",
                codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P"))
            .ToList();

        Transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(many);

        var result = await Build().SearchTransactions(limit: 500);

        using var doc = JsonDocument.Parse(Json(result.Data));
        doc.RootElement.GetProperty("rows").GetArrayLength().ShouldBe(30);
        doc.RootElement.GetProperty("totalMatches").GetInt32().ShouldBe(45);
    }

    [Fact]
    public async Task Busca_filtra_pelo_texto_da_descricao()
    {
        Transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DomainTransactions.Transaction>
            {
                new(userId: 1, amount: 45m, transactionDate: DateTime.Today.AddDays(-2), description: "iFood pedido",
                    codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P"),
                new(userId: 1, amount: 20m, transactionDate: DateTime.Today.AddDays(-3), description: "uber",
                    codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P")
            });

        var result = await Build().SearchTransactions(text: "ifood");

        using var doc = JsonDocument.Parse(Json(result.Data));
        doc.RootElement.GetProperty("totalMatches").GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task Abrir_tela_valida_registra_acao_e_invalida_nao_registra()
    {
        var toolbox = Build();

        (await toolbox.OpenScreen("dashboard")).Ok.ShouldBeTrue();
        (await toolbox.OpenScreen("invoices")).Ok.ShouldBeTrue();
        (await toolbox.OpenScreen("../admin")).Ok.ShouldBeFalse();

        _actions.Actions.Select(a => a.Path).ShouldBe(new[] { "/dashboard", "/credit-card/invoices" });
        _actions.Actions.ShouldAllBe(a => a.Type == AiChatActionTypes.Navigate);
    }

    [Fact]
    public async Task Tour_inclui_o_parametro_tour_e_nao_existe_para_faturas()
    {
        var toolbox = Build();

        (await toolbox.StartTour("invoices")).Ok.ShouldBeFalse();
        (await toolbox.StartTour("goals")).Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.StartTour);
        action.QueryParams!["tour"].ShouldBe("true");
    }

    [Fact]
    public async Task Pagamento_de_fatura_escolhe_a_fatura_aberta_mais_antiga_e_oferece_todas_as_formas()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice>
        {
            Invoice(102, 10, 2026, 11, 300m),
            Invoice(101, 10, 2026, 10, 250m),
            Invoice(99, 10, 2026, 8, 1614m),
            Invoice(100, 10, 2026, 9, 99m, paid: true)
        });
        Pending(101, 250m);
        Pending(102, 300m);

        var result = await Build().PrepareInvoicePayment("nubank");

        result.Ok.ShouldBeTrue();
        var proposal = _actions.Actions.Single().InvoicePayment!;
        proposal.CardInvoiceId.ShouldBe(101);
        proposal.Amount.ShouldBe(250m);
        proposal.PaymentMethods.Select(m => m.Description).ShouldBe(new[] { "Pix", "Debito" });
        result.Message.ShouldContain("NOT done yet");
        NothingWasWritten();
    }

    [Fact]
    public async Task Pagamento_de_fatura_com_forma_citada_oferece_so_aquela()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice> { Invoice(101, 10, 2026, 10, 250m) });
        Pending(101, 250m);

        await Build().PrepareInvoicePayment("nubank", paymentMethodName: "pix");

        _actions.Actions.Single().InvoicePayment!.PaymentMethods.Single().Description.ShouldBe("Pix");
    }

    [Fact]
    public async Task Pagamento_de_fatura_ja_paga_ou_inexistente_falha_sem_acao()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice> { Invoice(100, 10, 2026, 9, 99m, paid: true) });

        var paid = await Build().PrepareInvoicePayment("nubank", month: 9, year: 2026);
        var none = await Build().PrepareInvoicePayment("nubank", month: 1, year: 2020);

        paid.Ok.ShouldBeFalse();
        none.Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();
        NothingWasWritten();
    }

    [Fact]
    public async Task Pagamento_de_fatura_com_cartao_inexistente_ou_ambiguo_lista_as_opcoes()
    {
        var notFound = await Build().PrepareInvoicePayment("santander");
        notFound.Ok.ShouldBeFalse();
        notFound.Message.ShouldContain("Nubank Vitor");

        CreditCards.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<CreditCard>
        {
            new(10, 1, 1, "Cartao Vitor", 28, 20, 5000m),
            new(11, 1, 1, "Cartao Lany", 28, 20, 3000m)
        });

        var ambiguous = await Build().PrepareInvoicePayment("cartao");
        ambiguous.Ok.ShouldBeFalse();
        ambiguous.Message.ShouldContain("Ambiguous");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task So_uma_proposta_de_pagamento_por_mensagem()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice> { Invoice(101, 10, 2026, 10, 250m) });
        Invoices.Setup(i => i.GetByCardAsync(11, 1)).ReturnsAsync(new List<CardInvoice> { Invoice(201, 11, 2026, 10, 100m) });
        Invoices.Setup(i => i.GetPendingAmountsByCardAsync(11, 1)).ReturnsAsync(new Dictionary<int, decimal> { [201] = 100m });
        Pending(101, 250m);

        var toolbox = Build();

        (await toolbox.PrepareInvoicePayment("nubank")).Ok.ShouldBeTrue();
        var second = await toolbox.PrepareInvoicePayment("itau");

        second.Ok.ShouldBeFalse();
        second.Message.ShouldContain("Only one");
        _actions.Actions.Count(a => a.Type == AiChatActionTypes.ConfirmInvoicePayment).ShouldBe(1);
    }

    [Fact]
    public async Task Fatura_aberta_sem_valor_pendente_e_ignorada_e_a_proxima_com_valor_e_escolhida()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice>
        {
            Invoice(98, 10, 2026, 7, 1614m),
            Invoice(101, 10, 2026, 10, 250m)
        });
        Pending(101, 250m);

        var result = await Build().PrepareInvoicePayment("nubank");

        result.Ok.ShouldBeTrue();
        _actions.Actions.Single().InvoicePayment!.CardInvoiceId.ShouldBe(101);
    }

    [Fact]
    public async Task Pagamento_de_fatura_devolve_o_cartao_na_proposta()
    {
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice> { Invoice(101, 10, 2026, 10, 250m) });
        Pending(101, 250m);

        await Build().PrepareInvoicePayment("nubank");

        _actions.Actions.Single().InvoicePayment!.CreditCardId.ShouldBe(10);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", 50)]
    [InlineData("almoco", null)]
    [InlineData("almoco", 0)]
    [InlineData(null, 50)]
    public async Task Transacao_sem_descricao_ou_valor_pergunta_em_vez_de_abrir(string? description, int? amount)
    {
        var result = await Build().PrepareTransactions(new[] { Tx(description, amount) });

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("Missing required information");
        _actions.Actions.ShouldBeEmpty();
        NothingWasWritten();
    }

    [Fact]
    public async Task Transacao_sem_itens_pergunta()
    {
        (await Build().PrepareTransactions(null)).Message.ShouldContain("Missing required information");
        (await Build().PrepareTransactions(Array.Empty<TransactionToolItem>())).Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Transacao_completa_abre_o_modal_com_padroes_de_data_e_tipo()
    {
        var result = await Build().PrepareTransactions(new[] { Tx("  almoco  ", 50m) });

        result.Ok.ShouldBeTrue();
        result.Message.ShouldContain("NOTHING was saved");

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenTransactionsModal);
        action.Path.ShouldBe("/transaction");

        var draft = action.TransactionDrafts!.Single();
        draft.Description.ShouldBe("almoco");
        draft.Amount.ShouldBe(50m);
        draft.Type.ShouldBe("E");
        draft.Date.ShouldBe(BrazilClock.Today().ToString("yyyy-MM-dd"));
        NothingWasWritten();
    }

    [Fact]
    public async Task Transacao_resolve_forma_de_pagamento_e_categoria_pelos_nomes_e_ignora_desconhecidos()
    {
        (await Build().PrepareTransactions(new[] { Tx("feira", 80m, method: "pix", category: "mercado") })).Ok.ShouldBeTrue();

        var draft = _actions.Actions.Single().TransactionDrafts!.Single();
        draft.PaymentMethodId.ShouldBe(1);
        draft.CategoryId.ShouldBe(3);

        _actions = new ChatActionCollector();
        await Build().PrepareTransactions(new[] { Tx("feira", 80m, method: "cheque especial", category: "xyz") });

        var unknown = _actions.Actions.Single().TransactionDrafts!.Single();
        unknown.PaymentMethodId.ShouldBeNull();
        unknown.CategoryId.ShouldBeNull();
    }

    [Fact]
    public async Task Transacao_com_data_ou_tipo_invalidos_pergunta()
    {
        (await Build().PrepareTransactions(new[] { Tx(date: "ontem") })).Message.ShouldContain("date");
        (await Build().PrepareTransactions(new[] { Tx(type: "doacao") })).Message.ShouldContain("type");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Tipo_receita_vira_I_e_varias_transacoes_apontam_qual_item_falta()
    {
        (await Build().PrepareTransactions(new[] { Tx("salario", 3500m, type: "income") })).Ok.ShouldBeTrue();
        _actions.Actions.Single().TransactionDrafts!.Single().Type.ShouldBe("I");

        _actions = new ChatActionCollector();
        var partial = await Build().PrepareTransactions(new[] { Tx("cafe", 8m), Tx("uber", null) });

        partial.Ok.ShouldBeFalse();
        partial.Message.ShouldContain("item 2");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Compra_de_cartao_sem_cartao_pergunta_listando_os_cartoes()
    {
        var result = await Build().PrepareCardPurchases(null, new[] { new CardPurchaseToolItem("tenis", 300m) });

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("Nubank Vitor");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Compra_de_cartao_com_cartao_ambiguo_ou_inexistente_nao_abre()
    {
        (await Build().PrepareCardPurchases("santander", new[] { new CardPurchaseToolItem("tenis", 300m) })).Ok.ShouldBeFalse();

        CreditCards.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<CreditCard>
        {
            new(10, 1, 1, "Cartao Vitor", 28, 20, 5000m),
            new(11, 1, 1, "Cartao Lany", 28, 20, 3000m)
        });

        var ambiguous = await Build().PrepareCardPurchases("cartao", new[] { new CardPurchaseToolItem("tenis", 300m) });

        ambiguous.Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Compra_de_cartao_sem_descricao_ou_valor_pergunta()
    {
        var result = await Build().PrepareCardPurchases(
            "nubank", new[] { new CardPurchaseToolItem(null, 300m), new CardPurchaseToolItem("fone", null) });

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("item 1");
        result.Message.ShouldContain("item 2");
        _actions.Actions.ShouldBeEmpty();
        NothingWasWritten();
    }

    [Fact]
    public async Task Compra_de_cartao_completa_abre_o_modal_com_parcelas_limitadas_e_o_cartao_no_destino()
    {
        var result = await Build().PrepareCardPurchases("itau", new[] { new CardPurchaseToolItem("tenis", 300m, Installments: 3) });

        result.Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenCardPurchasesModal);
        action.CreditCardId.ShouldBe(11);
        action.QueryParams!["cardId"].ShouldBe("11");
        action.CardPurchasesPreview!.Purchases.Single().InstallmentsCount.ShouldBe((short)3);
        NothingWasWritten();
    }

    [Fact]
    public async Task Compra_de_cartao_aplica_a_categoria_citada_pelo_nome()
    {
        await Build().PrepareCardPurchases("itau", new[] { new CardPurchaseToolItem("feira", 80m, CategoryName: "mercado") });

        _actions.Actions.Single().CardPurchasesPreview!.Purchases.Single().CategoryId.ShouldBe(3);
    }

    [Fact]
    public async Task Categoria_sem_nome_ou_tipo_pergunta_e_valida_abre_o_modal()
    {
        (await Build().PrepareCategory(null, "expense")).Ok.ShouldBeFalse();
        (await Build().PrepareCategory("Pet", null)).Ok.ShouldBeFalse();
        (await Build().PrepareCategory("Pet", "talvez")).Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();

        (await Build().PrepareCategory("Pet", "income")).Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenCategoryModal);
        action.CategoryDraft!.Description.ShouldBe("Pet");
        action.CategoryDraft.Type.ShouldBe("I");
        action.QueryParams!["tab"].ShouldBe("categories");
        NothingWasWritten();
    }

    [Fact]
    public async Task Categoria_duplicada_e_recusada()
    {
        (await Build().PrepareCategory("mercado", "expense")).Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Subcategoria_pergunta_a_categoria_e_o_nome_e_recusa_duplicada()
    {
        (await Build().PrepareSubcategory(null, "feira")).Message.ShouldContain("Mercado");
        (await Build().PrepareSubcategory("xyz", "feira")).Message.ShouldContain("not found");
        (await Build().PrepareSubcategory("mercado", null)).Message.ShouldContain("subcategory name");

        CategoryItems.Setup(i => i.GetAllAsync(3)).ReturnsAsync(new[] { Item(3, "Feira") });
        (await Build().PrepareSubcategory("mercado", "feira")).Message.ShouldContain("already exists");

        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Subcategoria_valida_abre_o_modal_com_a_categoria_resolvida()
    {
        (await Build().PrepareSubcategory("mercado", "Feira")).Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenSubcategoryModal);
        action.SubcategoryDraft!.CategoryId.ShouldBe(3);
        action.SubcategoryDraft.Description.ShouldBe("Feira");
    }

    [Fact]
    public async Task Forma_de_pagamento_pergunta_o_nome_recusa_duplicada_e_abre_o_modal()
    {
        (await Build().PreparePaymentMethod(" ")).Ok.ShouldBeFalse();
        (await Build().PreparePaymentMethod("pix")).Message.ShouldContain("already exists");
        _actions.Actions.ShouldBeEmpty();

        (await Build().PreparePaymentMethod("Vale refeicao")).Ok.ShouldBeTrue();
        _actions.Actions.Single().PaymentMethodDraft!.Description.ShouldBe("Vale refeicao");
        _actions.Actions.Single().Type.ShouldBe(AiChatActionTypes.OpenPaymentMethodModal);
        NothingWasWritten();
    }

    [Theory]
    [InlineData(null, 5000, "2030-01-01")]
    [InlineData("viagem", null, "2030-01-01")]
    [InlineData("viagem", 5000, null)]
    [InlineData("viagem", 5000, "ano que vem")]
    [InlineData("viagem", 5000, "2020-01-01")]
    public async Task Meta_financeira_incompleta_ou_no_passado_pergunta(string? description, int? amount, string? date)
    {
        var result = await Build().PrepareFinancialGoal(description, amount, date);

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("Missing required information");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Meta_financeira_completa_abre_o_modal_com_prioridade_padrao_media()
    {
        (await Build().PrepareFinancialGoal("viagem", 5000m, "2030-06-01")).Ok.ShouldBeTrue();
        (await Build().PrepareFinancialGoal("carro", 20000m, "2031-01-01", "high")).Ok.ShouldBeTrue();

        var goals = _actions.Actions.Select(a => a.FinancialGoalDraft!).ToList();
        goals[0].Priority.ShouldBe(1);
        goals[1].Priority.ShouldBe(2);
        goals[0].TargetDate.ShouldBe("2030-06-01");
        _actions.Actions.ShouldAllBe(a => a.Type == AiChatActionTypes.OpenGoalModal && a.Path == "/goals");
        NothingWasWritten();
    }

    [Fact]
    public async Task Meta_de_orcamento_pergunta_categoria_e_limite_e_so_aceita_categoria_de_despesa()
    {
        (await Build().PrepareCategoryBudgetGoal(null, 500m)).Message.ShouldContain("Mercado");
        (await Build().PrepareCategoryBudgetGoal("salario", 500m)).Message.ShouldContain("not found");
        (await Build().PrepareCategoryBudgetGoal("mercado", null)).Message.ShouldContain("monthly limit");
        _actions.Actions.ShouldBeEmpty();

        (await Build().PrepareCategoryBudgetGoal("mercado", 500m)).Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenCategoryGoalModal);
        action.CategoryBudgetGoalDraft!.CategoryId.ShouldBe(3);
        action.CategoryBudgetGoalDraft.MonthlyLimit.ShouldBe(500m);
        NothingWasWritten();
    }

    [Fact]
    public async Task Visao_dos_cartoes_traz_limite_usado_e_faturas_em_aberto()
    {
        Purchases.Setup(p => p.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(1200m);
        Invoices.Setup(i => i.GetByCardAsync(10, 1)).ReturnsAsync(new List<CardInvoice>
        {
            Invoice(101, 10, 2026, 10, 250m),
            Invoice(98, 10, 2026, 7, 1614m),
            Invoice(100, 10, 2026, 9, 99m, paid: true)
        });
        Pending(101, 240m);

        var result = await Build().GetCardsOverview();

        var json = Json(result.Data);
        json.ShouldContain("Nubank Vitor");
        json.ShouldContain("1200");
        json.ShouldContain("2026-10");
        json.ShouldContain("240");
        json.ShouldNotContain("2026-09");
        json.ShouldNotContain("2026-07");
    }

    private static FinancialGoal Goal(int id, string description)
    {
        var goal = new FinancialGoal(1, description, 5000m, DateTime.Today.AddYears(1), global::Domain.Enums.Goals.GoalPriority.Medium);
        typeof(FinancialGoal).GetProperty("Id")!.SetValue(goal, id);
        return goal;
    }

    [Fact]
    public async Task Aporte_sem_metas_ativas_falha_sem_acao()
    {
        var result = await Build().PrepareGoalContribution("viagem", 200m);

        result.Ok.ShouldBeFalse();
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Aporte_sem_meta_ou_valor_pergunta_e_meta_inexistente_ou_ambigua_lista_as_metas()
    {
        _goals.Setup(g => g.GetActiveAsync(1)).ReturnsAsync(new List<FinancialGoal>
        {
            Goal(1, "Viagem Europa"),
            Goal(2, "Viagem Japao"),
            Goal(3, "Reserva")
        });

        (await Build().PrepareGoalContribution(null, 200m)).Message.ShouldContain("Reserva");
        (await Build().PrepareGoalContribution("carro", 200m)).Message.ShouldContain("not found");
        (await Build().PrepareGoalContribution("viagem", 200m)).Message.ShouldContain("Ambiguous");
        (await Build().PrepareGoalContribution("reserva", null)).Message.ShouldContain("contribution amount");
        (await Build().PrepareGoalContribution("reserva", 0m)).Ok.ShouldBeFalse();

        _actions.Actions.ShouldBeEmpty();
        NothingWasWritten();
    }

    [Fact]
    public async Task Aporte_completo_abre_o_modal_com_a_meta_resolvida_e_nao_grava()
    {
        _goals.Setup(g => g.GetActiveAsync(1)).ReturnsAsync(new List<FinancialGoal> { Goal(3, "Reserva") });

        var result = await Build().PrepareGoalContribution("reserva", 200m);

        result.Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenGoalContributionModal);
        action.Path.ShouldBe("/goals");
        action.GoalContributionDraft!.GoalId.ShouldBe(3);
        action.GoalContributionDraft.Amount.ShouldBe(200m);
        NothingWasWritten();
    }

    [Theory]
    [InlineData(null, 1500, "expense")]
    [InlineData("Aluguel", null, "expense")]
    [InlineData("Aluguel", 1500, null)]
    [InlineData("Aluguel", 1500, "talvez")]
    public async Task Compromisso_sem_descricao_valor_ou_direcao_pergunta(string? description, int? amount, string? direction)
    {
        var result = await Build().PreparePlannedCommitment(description, amount, direction);

        result.Ok.ShouldBeFalse();
        result.Message.ShouldContain("Missing required information");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Compromisso_completo_abre_o_modal_com_padroes_mensal_e_inicio_hoje()
    {
        var result = await Build().PreparePlannedCommitment("  Aluguel ", 1500m, "expense");

        result.Ok.ShouldBeTrue();

        var action = _actions.Actions.Single();
        action.Type.ShouldBe(AiChatActionTypes.OpenPlannedCommitmentModal);
        action.Path.ShouldBe("/planning");

        var draft = action.PlannedCommitmentDraft!;
        draft.Description.ShouldBe("Aluguel");
        draft.Amount.ShouldBe(1500m);
        draft.Direction.ShouldBe("O");
        draft.Periodicity.ShouldBe("M");
        draft.StartDate.ShouldBe(BrazilClock.Today().ToString("yyyy-MM-dd"));
        draft.EndDate.ShouldBeNull();
        draft.CategoryId.ShouldBeNull();
        NothingWasWritten();
    }

    [Fact]
    public async Task Compromisso_aceita_receita_periodicidade_datas_e_categoria_do_mesmo_tipo()
    {
        await Build().PreparePlannedCommitment("Salario", 4000m, "income", "annual", "2030-01-01", "2031-01-01", "salario");

        var draft = _actions.Actions.Single().PlannedCommitmentDraft!;
        draft.Direction.ShouldBe("I");
        draft.Periodicity.ShouldBe("A");
        draft.StartDate.ShouldBe("2030-01-01");
        draft.EndDate.ShouldBe("2031-01-01");
        draft.CategoryId.ShouldBe(5);

        _actions = new ChatActionCollector();
        await Build().PreparePlannedCommitment("Aluguel", 1500m, "expense", categoryName: "salario");

        _actions.Actions.Single().PlannedCommitmentDraft!.CategoryId.ShouldBeNull();
    }

    [Fact]
    public async Task Compromisso_com_periodicidade_ou_datas_invalidas_pergunta()
    {
        (await Build().PreparePlannedCommitment("Aluguel", 1500m, "expense", "quinzenal")).Message.ShouldContain("periodicity");
        (await Build().PreparePlannedCommitment("Aluguel", 1500m, "expense", startDate: "dia 5")).Message.ShouldContain("start date");
        (await Build().PreparePlannedCommitment("Aluguel", 1500m, "expense", startDate: "2030-02-01", endDate: "2030-01-01")).Message.ShouldContain("end date");
        _actions.Actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Compromisso_com_descricao_longa_e_truncada_para_60()
    {
        await Build().PreparePlannedCommitment(new string('a', 80), 10m, "expense");

        _actions.Actions.Single().PlannedCommitmentDraft!.Description.Length.ShouldBe(60);
    }
}
