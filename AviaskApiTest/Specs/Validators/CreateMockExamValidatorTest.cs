using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class CreateMockExamValidatorTest
{
    private readonly IValidator<CreateMockExamModel> _validator = new CreateMockExamValidator();

    [Fact]
    public async Task Valid()
    {
        var model = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 20,
            TimeLimit = TimeSpan.FromMinutes(30)
        };

        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MaxQuestions_Invalid_TooFew()
    {
        var model = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 19,
            TimeLimit = TimeSpan.FromMinutes(30)
        };

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task MaxQuestions_Invalid_TooMuch()
    {
        var model = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 19,
            TimeLimit = TimeSpan.FromMinutes(121)
        };

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task TimeLimit_Invalid_TooFew()
    {
        var model = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 20,
            TimeLimit = TimeSpan.FromMinutes(4)
        };

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task TimeLimit_Invalid_TooMuch()
    {
        var model = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 20,
            TimeLimit = TimeSpan.FromMinutes(121)
        };

        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}