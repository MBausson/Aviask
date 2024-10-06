using AviaskApi.Entities;
using FluentValidation;

namespace AviaskApi.Validators;

public class QuestionValidator : AbstractValidator<Question>
{
    public QuestionValidator()
    {
        RuleFor(q => q.Title).NotEmpty().Length(2, 128).WithMessage("Title must contain between 2 and 128 characters.");
        RuleFor(q => q.Description).NotEmpty().Length(2, 512)
            .WithMessage("Description must contain between 2 and 512 characters.");

        RuleFor(q => q.Category).NotEmpty().IsInEnum();

        RuleFor(q => q.Source).Length(2, 64).WithMessage("Source must contain between 2 and 42 characters.")
            .When(q => q.Source.Length != 0);

        RuleFor(q => q.QuestionAnswers).SetValidator(new QuestionAnswersValidator());

        RuleFor(q => q.Visibility).IsInEnum();
    }
}
