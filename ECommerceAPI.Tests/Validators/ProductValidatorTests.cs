using FluentAssertions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Validators;

namespace ECommerceAPI.Tests.Validators;

public sealed class ProductValidatorTests
{
    [Fact]
    public void CreateProductValidator_ShouldHaveError_WhenPriceIsZeroOrNegative()
    {
        var validator = new CreateProductRequestValidator();
        var model = new CreateProductRequestDto("Keyboard", "Wireless", 0m, 5, Guid.NewGuid());

        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateProductRequestDto.Price));
    }

    [Fact]
    public void CreateProductValidator_ShouldNotHaveErrors_WhenModelIsValid()
    {
        var validator = new CreateProductRequestValidator();
        var model = new CreateProductRequestDto("Keyboard", "Wireless", 120m, 5, Guid.NewGuid());

        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
