namespace Domain.Entities.Transactions
{
    public class StatusTransaction
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        protected StatusTransaction() { }

        public StatusTransaction(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
