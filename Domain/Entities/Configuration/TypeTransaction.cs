namespace KronPay.Domain.Entities.Configuration
{
    public class TypeTransaction
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        protected TypeTransaction() { }

        public TypeTransaction(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
