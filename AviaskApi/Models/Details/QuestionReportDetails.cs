using AviaskApi.Entities;

namespace AviaskApi.Models.Details;

public class QuestionReportDetails
{
    public Guid Id { get; set; }

    public string Message { get; set; } = null!;

    public ReportCategory Category { get; set; }

    public ReportState State { get; set; }

    public Guid IssuerId { get; set; }
    public AviaskUserDetails Issuer { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public QuestionDetails? Question { get; set; } = null!;
}