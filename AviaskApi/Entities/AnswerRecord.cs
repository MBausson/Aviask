using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AviaskApi.Models.Details;
using Newtonsoft.Json;

namespace AviaskApi.Entities;

public class AnswerRecord : IEntityDetails<AnswerRecordDetails>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Question))] public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    [ForeignKey(nameof(User))] public Guid UserId { get; set; }
    [JsonIgnore] public AviaskUser User { get; set; } = null!;

    public string Answered { get; set; } = null!;

    public bool IsCorrect { get; set; }

    [ForeignKey(nameof(MockExam))] public Guid? MockExamId { get; set; }
    public MockExam? MockExam { get; set; }

    public DateTime AnsweredAt { get; set; }

    public AnswerRecordDetails GetDetails()
    {
        return new AnswerRecordDetails
        {
            Id = Id,
            QuestionId = QuestionId,
            Question = Question.GetDetails(),
            UserId = UserId,
            User = User.GetDetails(),
            Answered = Answered,
            IsCorrect = IsCorrect,
            AnsweredAt = AnsweredAt
        };
    }

    public static AnswerRecord FromQuestionAndAnswer(Question question, AviaskUser user, string answer)
    {
        return new AnswerRecord
        {
            QuestionId = question.Id,
            Question = question,
            UserId = user.Id,
            User = user,
            Answered = answer,
            IsCorrect = question.IsCorrect(answer)
        };
    }
}