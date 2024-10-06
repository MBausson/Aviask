using AviaskApi.Models;
using AviaskApi.Validators;
using FluentValidation;

namespace AviaskApiTest.Specs.Validators;

public class LoginModelValidatorTest
{
    private readonly IValidator<LoginModel> _validator = new LoginModelValidator();

    [Fact]
    public async Task Valid()
    {
        var model = new LoginModel("email@example.com", "password123password");
        var result = await _validator.ValidateAsync(model);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Email_Valid()
    {
        var model = new LoginModel("email@@example.com", "password123password");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_Invalid_TooShort()
    {
        var model = new LoginModel("email@example.com", "pass");
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Password_Invalid_TooLong()
    {
        var model = new LoginModel("email@example.com", new string('p', 33));
        var result = await _validator.ValidateAsync(model);

        Assert.False(result.IsValid);
    }
}