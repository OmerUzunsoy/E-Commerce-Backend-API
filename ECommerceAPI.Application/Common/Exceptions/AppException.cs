namespace ECommerceAPI.Application.Common.Exceptions;

public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
