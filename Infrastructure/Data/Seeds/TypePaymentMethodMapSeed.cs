using Domain.Entities.Transactions;

namespace KronPay.Infra.Data.Seeds
{
    public static class StatusTransactionMapSeed
    {
        public static List<StatusTransaction> Data =>
        [
            new StatusTransaction("O", "Open"),
            new StatusTransaction("P", "Paid"),
            new StatusTransaction("C", "Canceled")
        ];
    }
}
