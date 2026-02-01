namespace KronPay.Domain.Entities.Configuration
{
    public class TypePaymentMethod
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        protected TypePaymentMethod() { }

        public TypePaymentMethod(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
