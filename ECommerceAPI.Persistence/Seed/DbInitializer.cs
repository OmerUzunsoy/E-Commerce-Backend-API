using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Persistence.Context;
using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(ECommerceDbContext context, IPasswordHasher<User> passwordHasher, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Roles.AnyAsync(cancellationToken))
        {
            return;
        }

        var adminRole = new Role { Name = "Admin" };
        var customerRole = new Role { Name = "Customer" };
        context.Roles.AddRange(adminRole, customerRole);

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@ecommerce.local",
            Role = adminRole
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

        var customer = new User
        {
            FullName = "Demo Customer",
            Email = "customer@ecommerce.local",
            Role = customerRole
        };
        customer.PasswordHash = passwordHasher.HashPassword(customer, "Customer123!");

        context.Users.AddRange(admin, customer);
        context.Carts.AddRange(new Cart { User = admin }, new Cart { User = customer });

        var categories = new[]
        {
            new Category { Name = "Electronics", Description = "Devices and accessories" },
            new Category { Name = "Home Office", Description = "Furniture and productivity tools" }
        };

        context.Categories.AddRange(categories);
        context.Products.AddRange(
            new Product
            {
                Name = "Mechanical Keyboard",
                Description = "Wireless mechanical keyboard with hot-swappable switches.",
                Price = 149.99m,
                Stock = 25,
                Category = categories[0]
            },
            new Product
            {
                Name = "4K Monitor",
                Description = "27-inch monitor suited for development and design work.",
                Price = 329.99m,
                Stock = 14,
                Category = categories[0]
            },
            new Product
            {
                Name = "Standing Desk",
                Description = "Motorized standing desk for a professional workspace.",
                Price = 549.00m,
                Stock = 8,
                Category = categories[1]
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
