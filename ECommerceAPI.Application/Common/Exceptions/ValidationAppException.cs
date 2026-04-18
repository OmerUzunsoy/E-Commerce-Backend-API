namespace ECommerceAPI.Application.Common.Exceptions;

public sealed class ValidationAppException(IDictionary<string, string[]> errors)
    : AppException("Validation failed.", 400)
{
    public new IDictionary<string, string[]> Errors { get; } = errors;
}
