using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Application.Abstractions.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequestDto request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequestDto request, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);
}
