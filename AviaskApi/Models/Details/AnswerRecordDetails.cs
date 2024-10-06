namespace AviaskApi.Models.Details;

public class AnswerRecordDetails
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }
    public QuestionDetails? Question { get; set; } = null!;

    public Guid UserId { get; set; }
    public AviaskUserDetails User { get; set; } = null!;

    public string Answered { get; set; } = null!;
    public bool IsCorrect { get; set; }

    public DateTime AnsweredAt { get; set; }
}