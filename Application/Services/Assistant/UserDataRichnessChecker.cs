using Domain.Interfaces;
using Domain.Interfaces.Transactions;

namespace Application.Services.Assistant;

public sealed class UserDataRichnessChecker
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICreditCardRepository _creditCardRepository;

    public UserDataRichnessChecker(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        ICreditCardRepository creditCardRepository)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _creditCardRepository = creditCardRepository;
    }

    public async Task<bool> HasEnoughDataAsync(int userId)
    {
        var categories = await _categoryRepository.GetAllAsync(userId);
        if (!categories.Any())
            return false;

        var now = DateTime.UtcNow;
        var transactionsThisMonth = await _transactionRepository.GetByMonthAsync(userId, now.Year, now.Month);
        if (transactionsThisMonth.Any())
            return true;

        var cards = await _creditCardRepository.GetAllAsync(userId);
        return cards.Any();
    }
}
