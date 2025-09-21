using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Modules.Identity.Application.DTOs.Responses
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
