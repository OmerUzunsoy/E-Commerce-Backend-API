using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Mappings;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class UserService(ECommerceDbContext context) : IUserService
{
    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return user.ToDto();
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Include(x => x.Role)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return users.Select(x => x.ToDto()).ToList();
    }

    public async Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var role = await context.Roles.FirstOrDefaultAsync(x => x.Name == request.RoleName, cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        user.RoleId = role.Id;
        user.Role = role;
        await context.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
