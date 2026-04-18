using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Application.Abstractions.Auth;

public interface ITokenService
{
    Task<AuthResponseDto> CreateTokensAsync(User user, CancellationToken cancellationToken = default);
}
