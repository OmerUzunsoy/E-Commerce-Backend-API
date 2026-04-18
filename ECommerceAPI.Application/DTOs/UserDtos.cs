namespace ECommerceAPI.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAtUtc);

public sealed record UpdateUserRoleRequestDto(string RoleName);
