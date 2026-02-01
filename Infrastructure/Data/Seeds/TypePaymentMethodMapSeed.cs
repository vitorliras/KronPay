using KronPay.Domain.Entities;
using KronPay.Domain.Entities.Configuration;

namespace KronPay.Infra.Data.Seeds
{
    public static class TypePaymentMethodMapSeed
    {
        public static List<TypePaymentMethod> Data =>
        [
            new TypePaymentMethod("R", "Receipt"),
            new TypePaymentMethod("P", "payment")
        ];
    }
}
