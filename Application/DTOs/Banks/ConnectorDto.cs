using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Banks
{
    public class ConnectorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string InstitutionUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public bool HasMFA { get; set; }
        public bool Oauth { get; set; }

        public ConnectorHealthDto Health { get; set; } = new();

        public List<string> Products { get; set; } = new();

        public bool IsSandbox { get; set; }
        public bool IsOpenFinance { get; set; }

        public bool SupportsPaymentInitiation { get; set; }
        public bool SupportsScheduledPayments { get; set; }
        public bool SupportsSmartTransfers { get; set; }
        public bool SupportsBoletoManagement { get; set; }
        public bool SupportsAutomaticPix { get; set; }

        public List<ConnectorCredentialDto> Credentials { get; set; } = new();
    }

}
