using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.banks
{
    public class BankAccount
    {
        public int Id { get; set; }

        public int BankConnectionId { get; set; }

        public string ExternalAccountId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal CurrentBalance { get; set; }

        public bool Active { get; set; }
        public virtual BankConnection BankConnection { get; set; } = null!;
    }
}
