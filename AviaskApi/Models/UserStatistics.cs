using AviaskApi.Entities;

namespace AviaskApi.Models;

public record CategoryStatistics(Category Category, float CorrectnessRatio, int AnswerCount);

public record DayActivity(DateTime Day, int AnswerCount);

public class UserStatistics
{
    public CategoryStatistics[] ReadyForExamCategories { get; set; } = null!;
    public CategoryStatistics[] WeakestCategories { get; set; } = null!;
    public CategoryStatistics[] TotalCategories { get; set; } = null!;
    public DayActivity[] Last30DaysCorrectAnswers { get; set; } = null!;
    public DayActivity[] Last30DaysWrongAnswers { get; set; } = null!;
}