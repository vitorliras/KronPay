using KronPay.Domain.Entities;
using KronPay.Domain.Entities.Configuration;

namespace KronPay.Infra.Data.Seeds
{
    public static class TypeTransactionSeed
    {
        public static List<TypeTransaction> Data =>
        [
            new TypeTransaction('I', "Income"),
            new TypeTransaction('E', "Expense"),
            new TypeTransaction('V', "Investment")
        ];
    }
}
