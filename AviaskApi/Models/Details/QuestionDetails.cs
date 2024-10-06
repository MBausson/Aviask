using AviaskApi.Entities;

namespace AviaskApi.Models.Details;

public record QuestionDetails(
    Guid Id,
    string Title,
    string Description,
    Category Category,
    string Source,
    List<string> Answers,
    Guid? PublisherId,
    DateTime PublishedAt,
    Guid? IllustrationId,
    QuestionStatus Status
);