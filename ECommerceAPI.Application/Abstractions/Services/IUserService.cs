using ECommerceAPI.Application.DTOs;

namespace ECommerceAPI.Application.Abstractions.Services;

public interface IUserService
{
    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default);
}
