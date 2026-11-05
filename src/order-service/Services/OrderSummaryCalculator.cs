using SahaPortal.OrderService.Data;

namespace SahaPortal.OrderService.Services;

/// <summary>Total demand for one product across all equipment requests.</summary>
public sealed record ProductSummary(string ProductId, int OrderCount, int TotalQuantity, string Level);

/// <summary>Groups equipment requests by product for the summary endpoint.</summary>
public static class OrderSummaryCalculator
{
    public const int MediumThreshold = 5;
    public const int HighThreshold = 20;

    public static IReadOnlyList<ProductSummary> Summarize(IEnumerable<Order> orders)
    {
        var totals = new Dictionary<string, (int Count, int Quantity)>(StringComparer.OrdinalIgnoreCase);

        foreach (var order in orders)
        {
            var key = order.ProductId.Trim().ToUpperInvariant();
            if (key.Length == 0)
            {
                continue;
            }

            totals.TryGetValue(key, out var current);
            totals[key] = (current.Count + 1, current.Quantity + Math.Max(order.Quantity, 0));
        }

        return totals
            .Select(pair => new ProductSummary(
                pair.Key, pair.Value.Count, pair.Value.Quantity, LevelFor(pair.Value.Quantity)))
            .OrderByDescending(summary => summary.TotalQuantity)
            .ThenBy(summary => summary.ProductId, StringComparer.Ordinal)
            .ToList();
    }

    public static string LevelFor(int totalQuantity)
    {
        if (totalQuantity <= 0)
        {
            return "none";
        }

        if (totalQuantity < MediumThreshold)
        {
            return "low";
        }

        return totalQuantity < HighThreshold ? "medium" : "high";
    }

    public static ProductSummary? Busiest(IReadOnlyList<ProductSummary> summaries)
    {
        return summaries.Count == 0 ? null : summaries[0];
    }
}
