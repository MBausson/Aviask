using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class PasswordResetModelValidator : AbstractValidator<PasswordResetModel>
{
    public PasswordResetModelValidator()
    {
        RuleFor(p => p.Token).NotEmpty();
        RuleFor(p => p.Password).NotEmpty().Length(6, 32)
            .WithMessage("The password must contain between 6 and 32 characters");
        RuleFor(p => p.RePassword).NotEmpty().Equal(p => p.Password).WithMessage("Both passwords don't match");
    }
}