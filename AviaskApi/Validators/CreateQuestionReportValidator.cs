using AviaskApi.Models;
using FluentValidation;

namespace AviaskApi.Validators;

public class CreateQuestionReportValidator : AbstractValidator<CreateQuestionReportModel>
{
    public CreateQuestionReportValidator()
    {
        RuleFor(q => q.Message).Length(6, 312)
            .WithMessage("The report message length must be between 6 and 312 characters.");

        RuleFor(q => q.Category).IsInEnum();

        RuleFor(q => q.QuestionId).NotEmpty();
    }
}