using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Persistence.Context;

public sealed class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(builder =>
        {
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(180).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Cart)
                .WithOne(x => x.User)
                .HasForeignKey<Cart>(x => x.UserId);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.Property(x => x.Token).HasMaxLength(200).IsRequired();
            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Category>(builder =>
        {
            builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(builder =>
        {
            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1500);
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(builder =>
        {
            builder.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<CartItem>(builder =>
        {
            builder.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId);
            builder.HasOne(x => x.Product)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Order>(builder =>
        {
            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            builder.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            builder.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId);
            builder.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
