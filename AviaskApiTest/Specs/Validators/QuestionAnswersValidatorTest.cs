using AviaskApi.Entities;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class QuestionAnswersValidatorTest
{
    private readonly QuestionAnswersFactory _factory = new();
    private readonly IValidator<QuestionAnswers> _validator = new QuestionAnswersValidator();

    [Fact]
    public async Task Valid()
    {
        var model = _factory.GetOne();
        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Answer1_Invalid_TooShort()
    {
        var model = _factory.GetOne();
        model.Answer1 = "";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Answer1_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Answer1 = new string('e', 129);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Answer2_Invalid()
    {
        var model = _factory.GetOne();
        model.Answer2 = "";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Answer3_Invalid()
    {
        var model = _factory.GetOne();
        model.Answer3 = "3";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Answer4_Invalid()
    {
        var model = _factory.GetOne();
        model.Answer4 = "4";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task CorrectAnswer_Invalid()
    {
        var model = _factory.GetOne();
        model.CorrectAnswer = "";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Explications_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Explications = new string('*', 513);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}
