using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class CreateUserModelValidator : AbstractValidator<CreateUserModel>
{
    public CreateUserModelValidator()
    {
        RuleFor(c => c.Username).NotEmpty().Length(3, 24)
            .WithMessage("Username must contain between 3 and 24 characters");

        RuleFor(c => c.Email).NotEmpty().EmailAddress()
            .WithMessage("The email address must be valid");

        RuleFor(c => c.Password).NotEmpty().Length(6, 32)
            .WithMessage("The password must contain between 6 and 32 characters");
    }
}