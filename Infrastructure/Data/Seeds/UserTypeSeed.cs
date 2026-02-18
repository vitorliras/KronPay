using KronPay.Domain.Entities;
using KronPay.Domain.Entities.Users;

namespace KronPay.Infra.Data.Seeds
{
    public static class UserTypeSeed
    {
        public static List<TypeUser> Data =>
        [
            new TypeUser("A", "Admin"),
            new TypeUser("P", "Premium"),
            new TypeUser("B", "Basic"),
            new TypeUser("V", "VIP"),
            new TypeUser("C", "Corporate"),
        ];
    }
}
