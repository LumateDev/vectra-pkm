using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Shared.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "vectra-api";
        public string Audience { get; set; } = "vectra-client";
        public int ExpiryMinutes { get; set; } = 60;
    }
}
