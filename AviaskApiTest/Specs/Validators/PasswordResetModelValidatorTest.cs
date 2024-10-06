using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class PasswordResetModelValidatorTest
{
    private readonly IValidator<PasswordResetModel> _validator = new PasswordResetModelValidator();

    [Fact]
    public async Task Valid()
    {
        var model = new PasswordResetModel("token", "password", "password");
        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Token_NotPresent()
    {
        var model = new PasswordResetModel("", "password", "password");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_NotPresent()
    {
        var model = new PasswordResetModel("token", "", "password");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_TooShort()
    {
        var model = new PasswordResetModel("token", "12345", "password");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RePassword_NotPresent()
    {
        var model = new PasswordResetModel("token", "password", "");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RePassword_TooShort()
    {
        var model = new PasswordResetModel("token", "password", "12345");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RePassword_DoesntMatch()
    {
        var model = new PasswordResetModel("token", "password", "password_");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}