using AviaskApi.Entities;

namespace AviaskApiTest.Specs.Entities;

public class MockExamTest
{
    private readonly MockExamFactory _factory = new();

    //  Since we cannot write on mockExam.AnswerRecords, we just put an absurdly high value on MaxQuestions
    [Fact]
    public void HasNextQuestion_True()
    {
        var mockExam = _factory.GetOne();
        mockExam.MaxQuestions = 2000;

        Assert.True(mockExam.HasNextQuestion());
    }

    [Fact]
    public void HasNextQuestion_False()
    {
        var mockExam = _factory.GetOne();
        mockExam.MaxQuestions = mockExam.AnswerRecords.Count;

        Assert.False(mockExam.HasNextQuestion());
    }

    [Fact]
    public void HasTimeRemaining_True()
    {
        var mockExam = _factory.GetOne();
        mockExam.StartedAt = DateTime.Now - TimeSpan.FromMinutes(15);
        mockExam.MaxDuration = TimeSpan.FromMinutes(30);

        Assert.True(mockExam.HasTimeRemaining());
    }

    [Fact]
    public void HasTimeRemaining_False()
    {
        var mockExam = _factory.GetOne();
        mockExam.StartedAt = DateTime.Now - TimeSpan.FromMinutes(31);
        mockExam.MaxDuration = TimeSpan.FromMinutes(30);

        Assert.False(mockExam.HasTimeRemaining());
    }

    [Fact]
    public void ProcessCorrectnessRatio_Empty()
    {
        var mockExam = new MockExam();

        Assert.Equal(0, mockExam.ProcessCorrectnessRatio());
    }

    [Fact]
    public void ProcessCorrectnessRatio_NotEmpty()
    {
        var mockExam = new MockExam
        {
            MaxQuestions = 20,
            AnswerRecords =
            {
                new AnswerRecord { IsCorrect = true }, new AnswerRecord { IsCorrect = true },
                new AnswerRecord { IsCorrect = true }, new AnswerRecord { IsCorrect = true },
                new AnswerRecord { IsCorrect = false }, new AnswerRecord { IsCorrect = false }
            }
        };

        Assert.Equal(0.2, mockExam.ProcessCorrectnessRatio(), 2);
    }
}