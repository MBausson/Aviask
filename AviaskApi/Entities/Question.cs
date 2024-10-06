using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AviaskApi.Models.Details;
using AviaskApi.Services.HtmlContentSanitizer;
using Newtonsoft.Json;

namespace AviaskApi.Entities;

public enum Category
{
    NULL = -1,
    AIR_LAW = 10,
    AIRFRAME_SYSTEMS = 21,
    INSTRUMENTATION = 22,
    MASS_AND_BALANCE = 31,
    PERFORMANCE = 32,
    FLIGHT_PLANNING = 33,
    HUMAN_PERFORMANCE = 40,
    METEOROLOGY = 50,
    GENERAL_NAVIGATION = 61,
    RADIO_NAVIGATION = 62,
    OPERATIONAL_PROCEDURES = 70,
    PRINCIPLES_OF_FLIGHT = 81,
    VFR_COMMUNICATIONS = 91,
    IFR_COMMUNICATIONS = 92
}

public enum QuestionVisibility
{
    //  Available to anybody
    PRIVATE,

    //  Available to logged in users, the default value
    PUBLIC
}

public enum QuestionStatus
{
    //  Accepted question
    ACCEPTED,

    //  Declined question, can't be accessed, doesn't appear in suggestions either
    DECLINED,

    //  Pending question, can only be accessed via suggestions by right-holders
    PENDING
}

public class Question : IEntityDetails<QuestionDetails>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Category Category { get; set; }
    public string Source { get; set; } = null!;

    [ForeignKey(nameof(QuestionAnswers))] public Guid QuestionAnswersId { get; set; }
    public QuestionAnswers QuestionAnswers { get; set; } = null!;

    [ForeignKey(nameof(Publisher))] public Guid? PublisherId { get; set; }
    [JsonIgnore] public AviaskUser? Publisher { get; set; }
    public DateTime PublishedAt { get; set; }

    public QuestionVisibility Visibility { get; set; } = QuestionVisibility.PRIVATE;
    public QuestionStatus Status { get; set; } = QuestionStatus.ACCEPTED;

    [ForeignKey("Attachment")] public Guid? IllustrationId { get; set; }

    public QuestionDetails GetDetails()
    {
        return new QuestionDetails(Id, Title, Description, Category, Source, QuestionAnswers.GetPossibleAnswers(),
            PublisherId, PublishedAt, IllustrationId, Status);
    }

    public bool IsCorrect(string check)
    {
        return QuestionAnswers.CorrectAnswer == check;
    }

    public void Sanitize(IHtmlContentSanitizerService sanitizer)
    {
        sanitizer.Sanitize(Description);
        sanitizer.Sanitize(QuestionAnswers.Explications ?? "");
    }
}
