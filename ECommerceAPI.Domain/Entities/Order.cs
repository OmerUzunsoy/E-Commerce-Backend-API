using ECommerceAPI.Domain.Common;
using ECommerceAPI.Domain.Enums;

namespace ECommerceAPI.Domain.Entities;

public sealed class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
