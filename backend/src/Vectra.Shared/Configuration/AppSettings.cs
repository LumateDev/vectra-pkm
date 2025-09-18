using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Shared.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "Vectra";

        public DatabaseSettings Database { get; set; } = new();
        public JwtSettings Jwt { get; set; } = new();
        public CorsSettings Cors { get; set; } = new();
        public string Environment { get; set; } = "Development";
    }
}
