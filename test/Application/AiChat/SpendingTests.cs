using Application.Services.Spending;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Moq;
using Shouldly;
using DomainTransactions = global::Domain.Entities.Transactions;

namespace Tests.Application.AiChat;

public class SpendingAggregatorTests
{
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ICardInvoiceRepository> _invoices = new();
    private readonly Mock<ICategoryRepository> _categories = new();

    private static Category Cat(int id, string name, bool invoice = false)
    {
        var category = new Category(1, name, "E");
        typeof(Category).GetProperty("Id")!.SetValue(category, id);

        if (invoice)
            category.MarkAsCardInvoiceCategory();

        return category;
    }

    private static DomainTransactions.Transaction Tx(string type, decimal amount, DateTime date, string description, int? categoryId, string status = "P") =>
        new(userId: 1, amount: amount, transactionDate: date, description: description,
            codTypeTransaction: type, categoryId: categoryId, categoryItemId: null, idPaymentMethod: 1, status: status);

    private SpendingAggregator Build(
        IEnumerable<DomainTransactions.Transaction> transactions,
        IEnumerable<CardInstallmentDetail>? installments = null,
        Category? invoiceCategory = null,
        IReadOnlyCollection<int>? paymentTransactionIds = null,
        params Category[] categories)
    {
        _transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((int _, DateTime from, DateTime to) =>
                transactions.Where(t => t.TransactionDate >= from && t.TransactionDate <= to).ToList());

        _invoices
            .Setup(i => i.GetInstallmentDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((installments ?? Array.Empty<CardInstallmentDetail>()).ToList());

        _invoices.Setup(i => i.GetPaymentTransactionIdsAsync(It.IsAny<int>())).ReturnsAsync(paymentTransactionIds ?? new List<int>());
        _categories.Setup(c => c.GetCardInvoiceCategoryAsync(It.IsAny<int>())).ReturnsAsync(invoiceCategory);
        _categories.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(categories.ToList());

        return new SpendingAggregator(_transactions.Object, _invoices.Object, _categories.Object);
    }

    private static CardInstallmentDetail Installment(short year, short month, decimal amount, string description, int? categoryId) =>
        new(year, month, description, amount, categoryId, null, 5, new DateTime(year, month, 3));

    [Fact]
    public async Task Considera_so_saidas_e_ignora_canceladas()
    {
        var aggregator = Build(
            new[]
            {
                Tx("E", 100m, new DateTime(2026, 10, 5), "mercado", 1),
                Tx("E", 40m, new DateTime(2026, 10, 6), "cancelada", 1, status: "C"),
                Tx("I", 900m, new DateTime(2026, 10, 7), "salario", 2),
                Tx("V", 50m, new DateTime(2026, 10, 8), "tesouro", 3)
            },
            categories: new[] { Cat(1, "Mercado") });

        var entries = await aggregator.GetExpenseEntriesAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        entries.Select(e => e.Description).ShouldBe(new[] { "mercado" });
    }

    [Fact]
    public async Task Exclui_a_transacao_de_pagamento_de_fatura_para_nao_contar_em_dobro()
    {
        var invoiceCategory = Cat(9, "Fatura cartao", invoice: true);

        var aggregator = Build(
            new[]
            {
                Tx("E", 1200m, new DateTime(2026, 10, 10), "Fatura Nubank - 10/2026", 9),
                Tx("E", 80m, new DateTime(2026, 10, 11), "padaria", 1)
            },
            installments: new[] { Installment(2026, 10, 1200m, "tenis", 1) },
            invoiceCategory: invoiceCategory,
            paymentTransactionIds: null,
            Cat(1, "Compras"), invoiceCategory);

        var totals = await aggregator.GetCategoryTotalsAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        totals.Sum(t => t.Total).ShouldBe(1280m);
        totals.ShouldNotContain(t => t.CategoryId == 9);
    }

    [Fact]
    public async Task Exclui_a_transacao_vinculada_a_fatura_paga_mesmo_sem_categoria_de_fatura()
    {
        var payment = Tx("E", 1200m, new DateTime(2026, 10, 10), "Fatura Nubank - 10/2026", null);
        typeof(DomainTransactions.Transaction).GetProperty("Id")!.SetValue(payment, 77);

        var aggregator = Build(
            new[] { payment, Tx("E", 80m, new DateTime(2026, 10, 11), "padaria", 1) },
            invoiceCategory: null,
            paymentTransactionIds: new List<int> { 77 },
            categories: Cat(1, "Compras"));

        var entries = await aggregator.GetExpenseEntriesAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        entries.Select(e => e.Description).ShouldBe(new[] { "padaria" });
    }

    [Fact]
    public async Task Parcela_do_cartao_entra_no_mes_da_fatura_no_primeiro_dia()
    {
        var aggregator = Build(
            Array.Empty<DomainTransactions.Transaction>(),
            installments: new[] { Installment(2026, 9, 300m, "tenis 1/3", 1) },
            categories: Cat(1, "Compras"));

        var entries = await aggregator.GetExpenseEntriesAsync(1, new DateTime(2026, 9, 1), new DateTime(2026, 9, 30));

        entries.Single().Date.ShouldBe(new DateTime(2026, 9, 1));
        entries.Single().FromCard.ShouldBeTrue();
    }

    [Fact]
    public async Task Soma_por_categoria_separa_a_parte_do_cartao_e_ordena_pelo_maior()
    {
        var aggregator = Build(
            new[]
            {
                Tx("E", 100m, new DateTime(2026, 10, 5), "mercado a", 1),
                Tx("E", 50m, new DateTime(2026, 10, 6), "bar", 2),
                Tx("E", 20m, new DateTime(2026, 10, 7), "sem categoria", null)
            },
            installments: new[] { Installment(2026, 10, 80m, "mercado b", 1) },
            categories: new[] { Cat(1, "Mercado"), Cat(2, "Lazer") });

        var totals = await aggregator.GetCategoryTotalsAsync(1, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        totals[0].Category.ShouldBe("Mercado");
        totals[0].Total.ShouldBe(180m);
        totals[0].CardPortion.ShouldBe(80m);
        totals.Last().Category.ShouldBe("(sem categoria)");
    }

    [Fact]
    public async Task Totais_mensais_trazem_despesa_receita_e_saldo_por_mes()
    {
        var aggregator = Build(
            new[]
            {
                Tx("E", 100m, new DateTime(2026, 9, 5), "a", 1),
                Tx("I", 1000m, new DateTime(2026, 9, 6), "salario", 2),
                Tx("E", 200m, new DateTime(2026, 10, 5), "b", 1),
                Tx("I", 1500m, new DateTime(2026, 10, 6), "salario", 2)
            },
            categories: Cat(1, "Mercado"));

        var months = await aggregator.GetMonthlyTotalsAsync(1, 2, new DateTime(2026, 10, 16));

        months.Select(m => m.Month).ShouldBe(new[] { "2026-09", "2026-10" });
        months[0].Expenses.ShouldBe(100m);
        months[0].Income.ShouldBe(1000m);
        months[1].Balance.ShouldBe(1300m);
    }

    [Fact]
    public async Task Informa_se_existe_categoria_de_pagamento_de_fatura()
    {
        (await Build(Array.Empty<DomainTransactions.Transaction>(), invoiceCategory: null).HasCardInvoiceCategoryAsync(1)).ShouldBeFalse();
        (await Build(Array.Empty<DomainTransactions.Transaction>(), invoiceCategory: Cat(9, "F", true)).HasCardInvoiceCategoryAsync(1)).ShouldBeTrue();
    }
}

public class RecurringExpenseDetectorTests
{
    private static SpendingEntry Entry(string description, DateTime date, decimal amount) =>
        new(date, description, amount, 1, null, false, null);

    [Fact]
    public void Detecta_gasto_que_aparece_em_tres_meses_com_valor_parecido()
    {
        var entries = new[]
        {
            Entry("Netflix", new DateTime(2026, 8, 5), 55.90m),
            Entry("NETFLIX", new DateTime(2026, 9, 5), 55.90m),
            Entry("netflix", new DateTime(2026, 10, 5), 57m)
        };

        var result = RecurringExpenseDetector.Detect(entries);

        result.Single().MonthsCount.ShouldBe(3);
        result.Single().AverageAmount.ShouldBeInRange(56m, 57m);
    }

    [Fact]
    public void Ignora_gasto_em_menos_de_tres_meses()
    {
        var entries = new[]
        {
            Entry("netflix", new DateTime(2026, 9, 5), 55m),
            Entry("netflix", new DateTime(2026, 10, 5), 55m)
        };

        RecurringExpenseDetector.Detect(entries).ShouldBeEmpty();
    }

    [Fact]
    public void Ignora_gasto_com_valor_muito_variavel()
    {
        var entries = new[]
        {
            Entry("mercado", new DateTime(2026, 8, 5), 50m),
            Entry("mercado", new DateTime(2026, 9, 5), 400m),
            Entry("mercado", new DateTime(2026, 10, 5), 120m)
        };

        RecurringExpenseDetector.Detect(entries).ShouldBeEmpty();
    }

    [Fact]
    public void Varias_compras_no_mesmo_mes_somam_antes_de_comparar()
    {
        var entries = new[]
        {
            Entry("agua", new DateTime(2026, 8, 5), 50m),
            Entry("agua", new DateTime(2026, 8, 20), 50m),
            Entry("agua", new DateTime(2026, 9, 5), 100m),
            Entry("agua", new DateTime(2026, 10, 5), 100m)
        };

        RecurringExpenseDetector.Detect(entries).Single().AverageAmount.ShouldBe(100m);
    }

    [Fact]
    public void Descricoes_sem_texto_util_sao_ignoradas()
    {
        var entries = Enumerable.Range(8, 3).Select(m => Entry("123", new DateTime(2026, m, 5), 10m));

        RecurringExpenseDetector.Detect(entries).ShouldBeEmpty();
    }
}
