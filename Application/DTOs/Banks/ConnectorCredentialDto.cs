using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Banks
{
    public class ConnectorCredentialDto
    {
        public string Label { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public string? Validation { get; set; }
        public string? ValidationMessage { get; set; }

        public string? Placeholder { get; set; }

        public bool Optional { get; set; }
    }

}
