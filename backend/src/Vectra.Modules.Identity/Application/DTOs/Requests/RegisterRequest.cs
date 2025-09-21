namespace Vectra.Modules.Identity.Application.DTOs.Requests
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
