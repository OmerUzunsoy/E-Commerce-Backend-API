using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ECommerceAPI.Application.Abstractions.Auth;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Application.Mappings;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Context;
using ECommerceAPI.Persistence.Identity;

namespace ECommerceAPI.Persistence.Services;

public sealed class TokenService(
    IOptions<JwtOptions> jwtOptions,
    ECommerceDbContext context) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponseDto> CreateTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        var roleName = user.Role?.Name ?? await context.Roles
            .Where(x => x.Id == user.RoleId)
            .Select(x => x.Name)
            .FirstAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(accessToken, refreshTokenValue, expiresAtUtc, user.ToDto());
    }
}
