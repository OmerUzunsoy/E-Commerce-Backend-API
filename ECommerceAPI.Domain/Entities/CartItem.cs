using ECommerceAPI.Domain.Common;

namespace ECommerceAPI.Domain.Entities;

public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
}
