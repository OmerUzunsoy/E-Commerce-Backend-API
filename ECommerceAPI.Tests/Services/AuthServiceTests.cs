using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using ECommerceAPI.Application.Abstractions.Auth;
using ECommerceAPI.Application.Common.Exceptions;
using ECommerceAPI.Application.DTOs;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Services;
using ECommerceAPI.Tests.Helpers;

namespace ECommerceAPI.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        await using var context = TestDbContextFactory.Create();
        var role = new Role { Name = "Customer" };
        var user = new User { FullName = "Jane Doe", Email = "jane@example.com", PasswordHash = "hashed", Role = role };
        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var passwordHasher = new Mock<IPasswordHasher<User>>();
        passwordHasher
            .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed", "Password123"))
            .Returns(PasswordVerificationResult.Success);

        var tokenService = new Mock<ITokenService>();
        tokenService
            .Setup(x => x.CreateTokensAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponseDto("access", "refresh", DateTime.UtcNow.AddMinutes(30), new UserDto(user.Id, user.FullName, user.Email, "Customer", user.CreatedAtUtc)));

        var sut = new AuthService(context, passwordHasher.Object, tokenService.Object);

        var result = await sut.LoginAsync(new LoginRequestDto("jane@example.com", "Password123"));

        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        tokenService.Verify(x => x.CreateTokensAsync(It.Is<User>(u => u.Email == "jane@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        await using var context = TestDbContextFactory.Create();
        var role = new Role { Name = "Customer" };
        var user = new User { FullName = "Jane Doe", Email = "jane@example.com", PasswordHash = "hashed", Role = role };
        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var passwordHasher = new Mock<IPasswordHasher<User>>();
        passwordHasher
            .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed", "WrongPassword"))
            .Returns(PasswordVerificationResult.Failed);

        var tokenService = new Mock<ITokenService>();
        var sut = new AuthService(context, passwordHasher.Object, tokenService.Object);

        var action = () => sut.LoginAsync(new LoginRequestDto("jane@example.com", "WrongPassword"));

        await action.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("Invalid email or password.");
        tokenService.Verify(x => x.CreateTokensAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenTokenIsInvalid()
    {
        await using var context = TestDbContextFactory.Create();
        var passwordHasher = new Mock<IPasswordHasher<User>>();
        var tokenService = new Mock<ITokenService>();
        var sut = new AuthService(context, passwordHasher.Object, tokenService.Object);

        var action = () => sut.RefreshTokenAsync(new RefreshTokenRequestDto("missing-token"));

        await action.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("Refresh token is invalid.");
    }
}
