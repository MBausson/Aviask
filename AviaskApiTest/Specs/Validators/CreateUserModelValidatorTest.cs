using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class CreateUserModelValidatorTest
{
    private readonly IValidator<CreateUserModel> _validator = new CreateUserModelValidator();

    [Fact]
    public async Task Valid()
    {
        var model = new CreateUserModel("Username", "email@example.com", "password123password");

        var result = await _validator.ValidateAsync(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Username_Invalid_TooShort()
    {
        var model = new CreateUserModel("", "email@example.com", "password123password");

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Username_Invalid_TooLong()
    {
        var model = new CreateUserModel(new string('a', 25), "email@example.com", "password123password");

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Email_Invalid()
    {
        var model = new CreateUserModel("Username", "email@@example.com", "password123password");

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_Invalid_TooShort()
    {
        var model = new CreateUserModel("Username", "email@example.com", "");

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_Invalid_TooLong()
    {
        var model = new CreateUserModel("Username", "email@@example.com", new string('p', 33));

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }
}