using FluentValidation;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Application.Validators;

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequestDto>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequestDto>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
