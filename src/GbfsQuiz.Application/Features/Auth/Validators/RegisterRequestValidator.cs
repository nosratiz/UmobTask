using FluentValidation;
using GbfsQuiz.Application.Features.Auth.Requests;

namespace GbfsQuiz.Application.Features.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().MinimumLength(3).MaximumLength(32)
            .Matches("^[a-zA-Z0-9_.-]+$")
            .WithMessage("Username may only contain letters, digits, '.', '_' and '-'.");
        RuleFor(x => x.DisplayName).MaximumLength(64);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
