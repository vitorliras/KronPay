
namespace Domain.Entities.banks
{
    public class BankConnection
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ExternalConnectionId { get; set; } = string.Empty;

        public string InstitutionCode { get; set; } = string.Empty;

        public string InstitutionName { get; set; } = string.Empty;

        public bool Active { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastSyncAt { get; set; }
        public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    }
}
