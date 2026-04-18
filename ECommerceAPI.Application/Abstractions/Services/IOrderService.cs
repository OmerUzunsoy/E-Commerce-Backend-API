using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Application.Abstractions.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default);
}
