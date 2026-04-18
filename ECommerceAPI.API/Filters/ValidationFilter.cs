using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using ECommerceAPI.Application.Common.Exceptions;

namespace ECommerceAPI.API.Filters;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IAsyncActionFilter
    where TRequest : class
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var argument = context.ActionArguments.Values.OfType<TRequest>().FirstOrDefault();
        if (argument is null)
        {
            await next();
            return;
        }

        ValidationResult validation = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
        if (validation.IsValid)
        {
            await next();
            return;
        }

        var errors = validation.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationAppException(errors);
    }
}
