using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Persistence.Context;

namespace ECommerceAPI.Tests.Helpers;

internal static class TestDbContextFactory
{
    public static ECommerceDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ECommerceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
