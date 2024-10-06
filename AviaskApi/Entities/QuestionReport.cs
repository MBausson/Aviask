using System.ComponentModel.DataAnnotations.Schema;
using AviaskApi.Models;
using AviaskApi.Models.Details;

namespace AviaskApi.Entities;

public enum ReportCategory
{
    INCORRECT_INFORMATION,
    TYPOS,
    RELEVANCE,
    INAPPROPRIATE_CONTENT,
    INCORRECT_EXPLICATIONS,
    FEEDBACK
}

public enum ReportState
{
    PENDING,
    TREATED,
    DECLINED
}

public class QuestionReport : IEntityDetails<QuestionReportDetails>
{
    public Guid Id { get; set; }

    public string Message { get; set; } = null!;

    public ReportCategory Category { get; set; }

    public ReportState State { get; set; }

    [ForeignKey(nameof(Issuer))] public Guid IssuerId { get; set; }
    public AviaskUser Issuer { get; set; } = null!;

    [ForeignKey(nameof(Question))] public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public QuestionReportDetails GetDetails()
    {
        return new QuestionReportDetails
        {
            Id = Id,
            Message = Message,
            Category = Category,
            State = State,
            QuestionId = QuestionId,
            Question = Question.GetDetails(),
            IssuerId = IssuerId,
            Issuer = Issuer.GetDetails()
        };
    }

    public static QuestionReport FromCreateModel(CreateQuestionReportModel model)
    {
        return new QuestionReport
        {
            Message = model.Message,
            QuestionId = model.QuestionId,
            Category = model.Category
        };
    }
}