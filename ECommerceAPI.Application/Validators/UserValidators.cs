using FluentValidation;
using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Application.Validators;

public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequestDto>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(50);
    }
}
