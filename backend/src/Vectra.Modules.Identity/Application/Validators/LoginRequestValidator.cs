using FluentValidation;
using Vectra.Modules.Identity.Application.DTOs.Requests;

namespace Vectra.Modules.Identity.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.EmailOrUsername)
                .NotEmpty().WithMessage("Email or username is required")
                .MaximumLength(256).WithMessage("Identifier is too long");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
