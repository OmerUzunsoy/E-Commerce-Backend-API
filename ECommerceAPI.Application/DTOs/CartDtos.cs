namespace ECommerceAPI.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record CartDto(Guid Id, Guid UserId, IReadOnlyCollection<CartItemDto> Items, decimal TotalAmount);

public sealed record AddCartItemRequestDto(Guid ProductId, int Quantity);

public sealed record UpdateCartItemRequestDto(int Quantity);
