using AviaskApi.Entities;

namespace AviaskApi.Models;

public class CreateMockExamModel
{
    public int MaxQuestions { get; set; }

    public Category Category { get; set; }

    public TimeSpan TimeLimit { get; set; }
}