namespace Saha.Common.Contracts;

/// <summary>An equipment request as returned by the order API.</summary>
public sealed record OrderDto(int Id, string ProductId, int Quantity, DateTime CreatedAt);
