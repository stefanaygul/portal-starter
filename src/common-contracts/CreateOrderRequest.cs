namespace Saha.Common.Contracts;

/// <summary>Body of POST /api/orders.</summary>
public sealed record CreateOrderRequest
{
    public string ProductId { get; init; } = string.Empty;

    public int Quantity { get; init; }
}
