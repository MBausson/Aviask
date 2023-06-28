namespace Aviask.Models.ResponseModels
{
    public record CheckAnswerResponse(int Id, bool IsCorrect, string CorrectAnswer, string Explication);
}
