﻿using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage("The email address must be valid");

        RuleFor(c => c.Password).NotEmpty().Length(6, 32)
            .WithMessage("The password must contain between 6 and 32 characters");
    }
}