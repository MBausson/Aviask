namespace AviaskApi.Services.HtmlContentSanitizer;

public interface IHtmlContentSanitizerService
{
    public string Sanitize(string content);
}