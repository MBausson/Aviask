using AviaskApi.Entities;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class QuestionValidatorTest
{
    private readonly QuestionFactory _factory = new();
    private readonly IValidator<Question> _validator = new QuestionValidator();

    [Fact]
    public async Task Valid()
    {
        var model = _factory.GetOne();
        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Title_Invalid_TooShort()
    {
        var model = _factory.GetOne();
        model.Title = "t";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Title_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Title = new string('t', 129);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Description_Invalid_TooShort()
    {
        var model = _factory.GetOne();
        model.Description = "d";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Description_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Title = new string('t', 513);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Category_Invalid()
    {
        var model = _factory.GetOne();
        model.Category = (Category)1000;

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Source_Invalid_TooShort()
    {
        var model = _factory.GetOne();
        model.Source = "s";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Source_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Source = new string('s', 129);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task QuestionAnswers_Invalid()
    {
        var model = _factory.GetOne();
        model.QuestionAnswers.Answer1 = "";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Visibility_Invalid()
    {
        var model = _factory.GetOne();
        model.Visibility = (QuestionVisibility)44;

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}
