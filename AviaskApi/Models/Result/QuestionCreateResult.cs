using AviaskApi.Models.Details;

namespace AviaskApi.Models.Result;

public record QuestionCreateResult(IEnumerable<string> ErrorMessages, QuestionDetails? Question);