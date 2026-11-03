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
}
