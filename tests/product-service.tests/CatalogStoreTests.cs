using System.Text.Json;
using SahaPortal.ProductService.Catalog;

namespace SahaPortal.ProductService.Tests;

public class CatalogStoreTests
{
    [Fact]
    public void Catalog_contains_the_expected_product_codes()
    {
        var catalog = CatalogStore.FromFile(CatalogStore.DefaultPath);

        var codes = catalog.GetAll().Select(p => p.Code).ToArray();

        Assert.Equal(["ADP-01", "KBL-02", "STD-03", "SRJ-04"], codes);
    }

    [Fact]
    public void Every_product_has_a_name()
    {
        var catalog = CatalogStore.FromFile(CatalogStore.DefaultPath);

        Assert.All(catalog.GetAll(), p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
    }

    [Fact]
    public void Catalog_file_states_the_stock_status_of_every_product()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CatalogStore.DefaultPath));

        // A missing field would silently be read as "not in stock".
        Assert.All(document.RootElement.EnumerateArray().ToArray(), item =>
        {
            var kind = item.GetProperty("inStock").ValueKind;
            Assert.True(kind is JsonValueKind.True or JsonValueKind.False);
        });
    }

    [Fact]
    public void Catalog_reads_the_stock_status()
    {
        var catalog = CatalogStore.FromFile(CatalogStore.DefaultPath);

        Assert.True(catalog.GetAll().Single(p => p.Code == "ADP-01").InStock);
    }
}
