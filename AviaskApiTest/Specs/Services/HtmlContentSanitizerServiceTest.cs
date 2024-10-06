using AviaskApi.Services.HtmlContentSanitizer;

namespace AviaskApiTest.Specs.Services;

public class HtmlContentSanitizerServiceTest
{
    private readonly IHtmlContentSanitizerService _htmlContentSanitizerService = new HtmlContentSanitizerService();

    [Theory]
    [InlineData("<script>alert('XSS !')</script>")]
    [InlineData("<a href=\"www.example.com\">Link</a>")]
    [InlineData("<img src=\"image.png\" />")]
    public void RemoveTag(string content)
    {
        var result = _htmlContentSanitizerService.Sanitize(content);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("p", "<p>A safe paragraph</p>")]
    [InlineData("h1", "<h1>A safe title</h1>")]
    public void AllowSafeTag(string tag, string content, bool closedTag = true)
    {
        var result = _htmlContentSanitizerService.Sanitize(content);

        Assert.Contains($"<{tag}>", result);

        if (closedTag) Assert.Contains($"</{tag}>", result);
    }
}