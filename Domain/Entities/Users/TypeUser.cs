namespace KronPay.Domain.Entities.Users
{
    public class TypeUser
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        protected TypeUser() { }

        public TypeUser(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
