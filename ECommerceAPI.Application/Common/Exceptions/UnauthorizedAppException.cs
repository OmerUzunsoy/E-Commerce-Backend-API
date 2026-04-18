namespace ECommerceAPI.Application.Common.Exceptions;

public sealed class UnauthorizedAppException(string message) : AppException(message, 401);
