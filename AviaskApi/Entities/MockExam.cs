using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AviaskApi.Models;
using Newtonsoft.Json;

namespace AviaskApi.Entities;

public enum MockExamStatus
{
    ONGOING,
    FINISHED
}

public class MockExam
{
    [Key] public Guid Id { get; set; }

    public MockExamStatus Status { get; set; }

    public Category Category { get; set; }

    public TimeSpan MaxDuration { get; set; }

    public int MaxQuestions { get; set; }

    public float CorrectnessRatio { get; set; }

    [ForeignKey(nameof(User))] public Guid UserId { get; set; }
    [JsonIgnore] public AviaskUser User { get; set; } = null!;

    public ICollection<AnswerRecord> AnswerRecords { get; } = new List<AnswerRecord>();

    [ForeignKey(nameof(Question))] public Guid? QuestionId { get; set; }
    [JsonIgnore] public Question? Question { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public float ProcessCorrectnessRatio()
    {
        if (AnswerRecords.Count == 0) return 0f;

        return (float)AnswerRecords.Count(a => a.IsCorrect) / MaxQuestions;
    }

    public bool HasNextQuestion()
    {
        return AnswerRecords.Count < MaxQuestions;
    }

    public bool HasTimeRemaining()
    {
        return DateTimeOffset.UtcNow - StartedAt < MaxDuration;
    }

    public static MockExam FromCreateModel(CreateMockExamModel model, AviaskUser user)
    {
        return new MockExam
        {
            Id = new Guid(),
            Category = model.Category,
            MaxDuration = model.TimeLimit,
            MaxQuestions = model.MaxQuestions,
            StartedAt = DateTime.UtcNow,
            CorrectnessRatio = 1,
            Status = MockExamStatus.ONGOING,
            User = user,
            UserId = user.Id
        };
    }
}