using Ganss.Xss;

namespace AviaskApi.Services.HtmlContentSanitizer;

public class HtmlContentSanitizerService : IHtmlContentSanitizerService
{
    private static readonly string[] DisallowedTags = ["a", "img"];
    private static readonly string[] DisallowedAttributes = ["src", "href"];
    private readonly HtmlSanitizer _sanitizer = new();

    public HtmlContentSanitizerService()
    {
        _sanitizer.AllowDataAttributes = false;

        foreach (var disallowedTag in DisallowedTags) _sanitizer.AllowedTags.Remove(disallowedTag);
        foreach (var disallowedAttribute in DisallowedAttributes) _sanitizer.AllowedTags.Remove(disallowedAttribute);
    }

    public string Sanitize(string content)
    {
        return _sanitizer.Sanitize(content);
    }
}