using AviaskApi.Entities;

namespace AviaskApi.Models;

public class CreateQuestionReportModel
{
    public string Message { get; set; } = null!;

    public ReportCategory Category { get; set; }

    public Guid QuestionId { get; set; }
}