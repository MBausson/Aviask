using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class CreateQuestionReportValidatorTest
{
    private readonly CreateQuestionReportModelFactory _factory = new();
    private readonly IValidator<CreateQuestionReportModel> _validator = new CreateQuestionReportValidator();

    [Fact]
    public async Task Valid()
    {
        var model = _factory.GetOne();
        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Message_Invalid_TooShort()
    {
        var model = _factory.GetOne();
        model.Message = "";

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Message_Invalid_TooLong()
    {
        var model = _factory.GetOne();
        model.Message = new string('*', 313);

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Category_Invalid()
    {
        var model = _factory.GetOne();
        model.Category = (ReportCategory)200;

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task QuestionId_Invalid()
    {
        var model = _factory.GetOne();
        model.QuestionId = Guid.Empty;

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}