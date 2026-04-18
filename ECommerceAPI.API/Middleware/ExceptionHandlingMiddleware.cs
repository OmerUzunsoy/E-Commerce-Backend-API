using System.Text.Json;
using ECommerceAPI.Application.Common.Exceptions;

namespace ECommerceAPI.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        object response = new { message = "An unexpected error occurred." };

        if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            response = new
            {
                message = appException.Message,
                errors = exception is ValidationAppException validationException ? validationException.Errors : appException.Errors
            };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
