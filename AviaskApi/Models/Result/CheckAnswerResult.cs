namespace AviaskApi.Models.Result;

public record CheckAnswerResult(Guid QuestionId, bool IsCorrect, string CorrectAnswer, string? Explications);