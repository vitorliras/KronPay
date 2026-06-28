using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Banks
{
    public class ConnectorsResponseDto
    {
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }

        public List<ConnectorDto> Results { get; set; } = new();
    }

}
