using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class CreateMockExamValidator : AbstractValidator<CreateMockExamModel>
{
    public CreateMockExamValidator()
    {
        RuleFor(m => m.Category).IsInEnum();

        RuleFor(m => m.MaxQuestions).InclusiveBetween(20, 120);

        RuleFor(m => m.TimeLimit).GreaterThanOrEqualTo(TimeSpan.FromMinutes(5));

        RuleFor(m => m.TimeLimit).LessThanOrEqualTo(TimeSpan.FromHours(2));
    }
}