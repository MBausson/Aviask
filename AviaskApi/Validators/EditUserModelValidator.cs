using AviaskApi.Entities;
using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class EditUserModelValidator : AbstractValidator<EditUserModel>
{
    public EditUserModelValidator()
    {
        RuleFor(u => u.Username).NotEmpty().Length(3, 24)
            .WithMessage("Username must contain between 3 and 24 characters");
        RuleFor(u => u.Email).NotEmpty().EmailAddress().WithMessage("The email address must be valid");
        RuleFor(u => u.Role).NotEmpty().Must(r => AviaskUserRole.AvailableRoles.Contains(r))
            .WithMessage("The role must be valid");
    }
}