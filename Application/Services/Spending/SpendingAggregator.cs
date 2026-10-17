using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;

namespace Application.Services.Spending;

public sealed class SpendingAggregator
{
    private const string Expense = "E";
    private const string Income = "I";
    private const string Cancelled = "C";

    private readonly ITransactionRepository _transactionRepository;
    private readonly ICardInvoiceRepository _invoiceRepository;
    private readonly ICategoryRepository _categoryRepository;

    public SpendingAggregator(
        ITransactionRepository transactionRepository,
        ICardInvoiceRepository invoiceRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _invoiceRepository = invoiceRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> HasCardInvoiceCategoryAsync(int userId) =>
        await _categoryRepository.GetCardInvoiceCategoryAsync(userId) is not null;

    public async Task<IReadOnlyList<SpendingEntry>> GetExpenseEntriesAsync(int userId, DateTime from, DateTime to)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1).AddTicks(-1);

        var invoiceCategory = await _categoryRepository.GetCardInvoiceCategoryAsync(userId);
        var paymentTransactionIds = (await _invoiceRepository.GetPaymentTransactionIdsAsync(userId)).ToHashSet();

        var transactions = (await _transactionRepository.GetByPeriodAsync(userId, start, end))
            .Where(t => t.CodTypeTransaction == Expense
                        && t.Status != Cancelled
                        && !paymentTransactionIds.Contains(t.Id)
                        && (invoiceCategory is null || t.CategoryId != invoiceCategory.Id))
            .Select(t => new SpendingEntry(
                t.TransactionDate,
                t.Description,
                t.Amount,
                t.CategoryId,
                t.CategoryItemId,
                false,
                null));

        var installments = (await _invoiceRepository.GetInstallmentDetailsAsync(
                userId, start.Year, start.Month, end.Year, end.Month))
            .Select(i => new SpendingEntry(
                new DateTime(i.InvoiceYear, i.InvoiceMonth, 1),
                i.Description,
                i.Amount,
                i.CategoryId,
                i.CategoryItemId,
                true,
                i.CreditCardId));

        return transactions.Concat(installments).OrderBy(e => e.Date).ToList();
    }

    public async Task<IReadOnlyList<CategoryTotal>> GetCategoryTotalsAsync(int userId, DateTime from, DateTime to)
    {
        var entries = await GetExpenseEntriesAsync(userId, from, to);
        var names = await CategoryNamesAsync(userId);

        return SumByCategory(entries, names);
    }

    public async Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(int userId, int months, DateTime today)
    {
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-(months - 1));

        var expenses = await GetExpenseEntriesAsync(userId, firstMonth, today);

        var incomes = (await _transactionRepository.GetByPeriodAsync(
                userId, firstMonth, today.Date.AddDays(1).AddTicks(-1)))
            .Where(t => t.CodTypeTransaction == Income && t.Status != Cancelled)
            .ToList();

        return Enumerable.Range(0, months)
            .Select(offset =>
            {
                var month = firstMonth.AddMonths(offset);

                var spent = expenses
                    .Where(e => e.Date.Year == month.Year && e.Date.Month == month.Month)
                    .Sum(e => e.Amount);

                var earned = incomes
                    .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                    .Sum(t => t.Amount);

                return new MonthlyTotal($"{month:yyyy-MM}", spent, earned, earned - spent);
            })
            .ToList();
    }

    public async Task<IReadOnlyDictionary<int, string>> CategoryNamesAsync(int userId) =>
        (await _categoryRepository.GetAllAsync(userId)).ToDictionary(c => c.Id, c => c.Description);

    public static IReadOnlyList<CategoryTotal> SumByCategory(
        IEnumerable<SpendingEntry> entries,
        IReadOnlyDictionary<int, string> names)
    {
        return entries
            .GroupBy(e => e.CategoryId)
            .Select(g => new CategoryTotal(
                g.Key,
                g.Key is { } id && names.TryGetValue(id, out var name) ? name : "(sem categoria)",
                g.Sum(e => e.Amount),
                g.Count(),
                g.Where(e => e.FromCard).Sum(e => e.Amount)))
            .OrderByDescending(c => c.Total)
            .ToList();
    }
}
