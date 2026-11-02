using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Saha.Common.Contracts;

namespace SahaPortal.ProductService.Tests;

public class ProductsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Get_products_returns_the_catalog()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        Assert.NotNull(products);
        Assert.Contains(products, p => p is { Code: "ADP-01", Name: "Terminal adaptörü" });
    }

    [Fact]
    public async Task Get_products_keeps_turkish_characters_readable()
    {
        var json = await _client.GetStringAsync("/api/products");

        Assert.Contains("Terminal adaptörü", json);
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
