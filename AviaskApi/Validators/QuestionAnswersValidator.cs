using AviaskApi.Entities;
using FluentValidation;

namespace AviaskApi.Validators;

public class QuestionAnswersValidator : AbstractValidator<QuestionAnswers>
{
    public QuestionAnswersValidator()
    {
        RuleFor(q => q.Answer1).NotEmpty().Length(2, 128)
            .WithMessage("Answer 1 must contain between 3 and 42 characters.");

        RuleFor(q => q.Answer2).NotEmpty().Length(2, 128)
            .WithMessage("Answer 2 must contain between 2 and 128 characters.");

        RuleFor(q => q.Answer3).Length(2, 128).When(q => q.Answer3?.Length != 0)
            .WithMessage("Answer 3 must contain between 2 and 128 characters.");
        RuleFor(q => q.Answer4).Length(2, 128).When(q => q.Answer4?.Length != 0)
            .WithMessage("Answer 4 must contain between 2 and 128 characters.");

        RuleFor(q => q.CorrectAnswer).Must((model, field) => model.GetPossibleAnswers().Contains(field))
            .WithMessage("The correct answer must be present among the possible answers.");

        RuleFor(q => q.Explications).Length(0, 512).When(q => q.Explications?.Length != 0)
            .WithMessage("Explications must contain between at most 512 characters.");
    }
}
