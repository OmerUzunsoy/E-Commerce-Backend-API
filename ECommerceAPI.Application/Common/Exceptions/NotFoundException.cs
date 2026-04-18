namespace ECommerceAPI.Application.Common.Exceptions;

public sealed class NotFoundException(string message) : AppException(message, 404);
