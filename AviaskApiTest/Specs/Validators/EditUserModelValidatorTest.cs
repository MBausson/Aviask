using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class EditUserModelValidatorTest
{
    private readonly IValidator<EditUserModel> _validator = new EditUserModelValidator();

    [Fact]
    public async Task Valid()
    {
        var model = new EditUserModel("Username", "email@example.com", "admin", false);

        var result = await _validator.ValidateAsync(model);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Username_Invalid_TooShort()
    {
        var model = new EditUserModel("", "email@example.com", "admin", false);

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Username_Invalid_TooLong()
    {
        var model = new EditUserModel(new string('a', 25), "email@example.com", "member", false);

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Email_Invalid()
    {
        var model = new EditUserModel("Username", "email@@example.com", "manager", false);

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Role_Invalid()
    {
        var model = new EditUserModel("Username", "email@example.com", "membered", false);

        var result = await _validator.ValidateAsync(model);
        Assert.False(result.IsValid);
    }
}