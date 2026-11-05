using System.Net.Http.Json;
using System.Text.Json;
using Saha.Common.Contracts;
using SahaPortal.OrderService.Data;
using SahaPortal.OrderService.Services;

namespace SahaPortal.OrderService.Tests;

public class OrderSummaryCalculatorTests
{
    private static Order NewOrder(string productId, int quantity) =>
        new() { ProductId = productId, Quantity = quantity, CreatedAt = DateTime.UtcNow };

    [Fact]
    public void Summarize_groups_orders_by_product_and_sorts_by_total_quantity()
    {
        var summaries = OrderSummaryCalculator.Summarize(
        [
            NewOrder("KBL-02", 3),
            NewOrder("ADP-01", 10),
            NewOrder("adp-01 ", 15),
            NewOrder("STD-03", 3),
        ]);

        Assert.Equal(["ADP-01", "KBL-02", "STD-03"], summaries.Select(s => s.ProductId).ToArray());
        Assert.Equal(2, summaries[0].OrderCount);
        Assert.Equal(25, summaries[0].TotalQuantity);
        Assert.Equal("high", summaries[0].Level);
    }

    [Fact]
    public void Summarize_ignores_blank_product_ids_and_negative_quantities()
    {
        var summaries = OrderSummaryCalculator.Summarize([NewOrder("  ", 4), NewOrder("ADP-01", -2)]);

        var only = Assert.Single(summaries);
        Assert.Equal("ADP-01", only.ProductId);
        Assert.Equal(0, only.TotalQuantity);
        Assert.Equal("none", only.Level);
    }

    [Theory]
    [InlineData(0, "none")]
    [InlineData(-1, "none")]
    [InlineData(1, "low")]
    [InlineData(4, "low")]
    [InlineData(5, "medium")]
    [InlineData(19, "medium")]
    [InlineData(20, "high")]
    public void LevelFor_maps_total_quantity_to_a_level(int totalQuantity, string expected)
    {
        Assert.Equal(expected, OrderSummaryCalculator.LevelFor(totalQuantity));
    }

    [Fact]
    public void Busiest_returns_the_first_summary_or_null()
    {
        Assert.Null(OrderSummaryCalculator.Busiest([]));

        var summaries = OrderSummaryCalculator.Summarize([NewOrder("KBL-02", 1), NewOrder("ADP-01", 7)]);
        Assert.Equal("ADP-01", OrderSummaryCalculator.Busiest(summaries)?.ProductId);
    }
}

public class OrderSummaryApiTests(OrderApiFactory factory) : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_summary_reports_totals_per_product()
    {
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { ProductId = "ADP-01", Quantity = 6 });
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest { ProductId = "KBL-02", Quantity = 2 });

        var summary = await _client.GetFromJsonAsync<JsonElement>("/api/orders/summary");

        Assert.Equal(2, summary.GetProperty("totalOrders").GetInt32());
        Assert.Equal("ADP-01", summary.GetProperty("busiest").GetString());
        Assert.Equal(2, summary.GetProperty("products").GetArrayLength());
        Assert.Equal("medium", summary.GetProperty("products")[0].GetProperty("level").GetString());
    }
}
