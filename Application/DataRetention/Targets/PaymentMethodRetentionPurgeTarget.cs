using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;

namespace Application.DataRetention.Targets;

public sealed class PaymentMethodRetentionPurgeTarget : IRetentionPurgeTarget
{
    private readonly IPaymentMethodRepository _paymentMethods;
    private readonly ITransactionRepository _transactions;

    public PaymentMethodRetentionPurgeTarget(
        IPaymentMethodRepository paymentMethods,
        ITransactionRepository transactions)
    {
        _paymentMethods = paymentMethods;
        _transactions = transactions;
    }

    public async Task<int> PurgeAsync(DateTime cutoff)
    {
        var candidates = await _paymentMethods.GetDeactivatedOlderThanAsync(cutoff);
        if (candidates.Count == 0)
            return 0;

        var deletable = new List<PaymentMethod>();

        foreach (var method in candidates)
        {
            var referenced = await _transactions.ExistsByPaymentMethodIdAsync(method.Id);
            if (!referenced)
                deletable.Add(method);
        }

        if (deletable.Count == 0)
            return 0;

        await _paymentMethods.DeleteRangeAsync(deletable);
        return deletable.Count;
    }
}
