using FluentAssertions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Validators;

namespace ECommerceAPI.Tests.Validators;

public sealed class AuthValidatorTests
{
    [Fact]
    public void RegisterValidator_ShouldRequireValidEmail()
    {
        var validator = new RegisterRequestValidator();
        var model = new RegisterRequestDto("Jane Doe", "not-an-email", "Secure123!");

        var result = validator.Validate(model);

        result.Errors.Should().Contain(x => x.PropertyName == nameof(RegisterRequestDto.Email));
    }

    [Fact]
    public void LoginValidator_ShouldRequirePassword()
    {
        var validator = new LoginRequestValidator();
        var model = new LoginRequestDto("jane@example.com", string.Empty);

        var result = validator.Validate(model);

        result.Errors.Should().Contain(x => x.PropertyName == nameof(LoginRequestDto.Password));
    }
}
