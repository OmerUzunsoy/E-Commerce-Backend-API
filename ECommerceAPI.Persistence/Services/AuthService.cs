using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Application.Abstractions.Auth;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Persistence.Services;

public sealed class AuthService(
    ECommerceDbContext context,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await context.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new AppException("Email is already in use.");
        }

        var customerRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == "Customer", cancellationToken)
            ?? throw new NotFoundException("Customer role not found.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            RoleId = customerRole.Id,
            Role = customerRole
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        context.Carts.Add(new Cart { User = user });
        await context.SaveChangesAsync(cancellationToken);

        return await tokenService.CreateTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid email or password.");

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        return await tokenService.CreateTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .Include(x => x.User!)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAppException("Refresh token is invalid.");

        if (token.IsRevoked || token.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("Refresh token is expired or revoked.");
        }

        token.IsRevoked = true;
        token.RevokedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return await tokenService.CreateTokensAsync(token.User!, cancellationToken);
    }

    public async Task LogoutAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);
        if (token is null)
        {
            return;
        }

        token.IsRevoked = true;
        token.RevokedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
