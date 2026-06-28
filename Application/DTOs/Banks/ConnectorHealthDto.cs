using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Banks
{
    public class ConnectorHealthDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Stage { get; set; }
    }

}
