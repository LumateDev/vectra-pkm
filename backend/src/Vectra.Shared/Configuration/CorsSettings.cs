using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Shared.Configuration
{
    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE" };
        public string[] AllowedHeaders { get; set; } = new[] { "Content-Type", "Authorization" };
    }
}
