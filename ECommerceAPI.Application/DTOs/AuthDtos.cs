namespace ECommerceAPI.Application.DTOs;

public sealed record RegisterRequestDto(string FullName, string Email, string Password);

public sealed record LoginRequestDto(string Email, string Password);

public sealed record RefreshTokenRequestDto(string RefreshToken);

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    UserDto User);
