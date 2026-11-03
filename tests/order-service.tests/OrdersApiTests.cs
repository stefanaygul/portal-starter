using System.Net;
using System.Net.Http.Json;
using Saha.Common.Contracts;

namespace SahaPortal.OrderService.Tests;

public class OrdersApiTests(OrderApiFactory factory) : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Post_order_returns_201_with_the_created_order()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/orders", new CreateOrderRequest { ProductId = "ADP-01", Quantity = 2 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.True(order.Id > 0);
        Assert.Equal("ADP-01", order.ProductId);
        Assert.Equal(2, order.Quantity);
        Assert.Equal($"/api/orders/{order.Id}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_order_without_a_product_id_returns_400(string productId)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/orders", new CreateOrderRequest { ProductId = productId, Quantity = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Ekipman kodu", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_orders_lists_previously_created_orders()
    {
        await _client.PostAsJsonAsync(
            "/api/orders", new CreateOrderRequest { ProductId = "KBL-02", Quantity = 5 });

        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/orders");

        Assert.NotNull(orders);
        Assert.Contains(orders, o => o is { ProductId: "KBL-02", Quantity: 5 });
    }

    [Fact]
    public async Task Get_orders_returns_creation_time_as_utc()
    {
        await _client.PostAsJsonAsync(
            "/api/orders", new CreateOrderRequest { ProductId = "STD-03", Quantity = 1 });

        var json = await _client.GetStringAsync("/api/orders");
        var orders = await _client.GetFromJsonAsync<List<OrderDto>>("/api/orders");

        Assert.Matches("\"createdAt\":\"[^\"]+Z\"", json);
        Assert.NotNull(orders);
        Assert.All(orders, o => Assert.True(DateTime.UtcNow - o.CreatedAt < TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
