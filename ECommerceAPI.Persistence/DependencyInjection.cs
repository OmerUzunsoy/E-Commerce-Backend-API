using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerceAPI.Application.Abstractions.Auth;
using ECommerceAPI.Application.Abstractions.Caching;
using ECommerceAPI.Application.Abstractions.Services;
using ECommerceAPI.Application.Options;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Persistence.Caching;
using ECommerceAPI.Persistence.Context;
using ECommerceAPI.Persistence.Identity;
using ECommerceAPI.Persistence.Services;

namespace ECommerceAPI.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "ecommerce-api:";
            });
        }

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
