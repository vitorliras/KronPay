namespace KronPay.Domain.Entities.Configuration
{
    public class TypeTransaction
    {
        public char Codigo { get; private set; }
        public string Descricao { get; private set; }

        protected TypeTransaction() { }

        public TypeTransaction(char codigo, string descricao)
        {
            Codigo = codigo;
            Descricao = descricao;
        }
    }
}
