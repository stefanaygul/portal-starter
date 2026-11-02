namespace SahaPortal.OrderService.Data;

public sealed class Order
{
    public int Id { get; set; }

    public required string ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
}
