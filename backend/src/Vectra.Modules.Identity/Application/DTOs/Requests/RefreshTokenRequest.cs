using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Modules.Identity.Application.DTOs.Requests
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
        public string? IpAddress { get; set; }  // Для аудита
    }
}
