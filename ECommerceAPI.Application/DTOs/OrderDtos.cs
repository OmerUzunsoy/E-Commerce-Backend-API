using ECommerceAPI.Domain.Enums;

namespace ECommerceAPI.Application.DTOs;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    string CustomerName,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record UpdateOrderStatusRequestDto(OrderStatus Status);
