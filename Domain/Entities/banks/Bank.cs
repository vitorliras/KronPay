using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.banks
{
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrimaryColor { get; set; }
        public string InstitutionUrl { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Active { get; set; }

        public Bank(int id, string name, string primaryColor, string institutionUrl, string? imageUrl, string type, bool active)
        {
            Id = id;
            Name = name;
            PrimaryColor = primaryColor;
            InstitutionUrl = institutionUrl;
            ImageUrl = imageUrl;
            Type = type;
            Active = active;
        }

    }
}
