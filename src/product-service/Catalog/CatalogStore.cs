using System.Text.Json;
using Saha.Common.Contracts;

namespace SahaPortal.ProductService.Catalog;

/// <summary>Reads the equipment catalog from a static JSON file.</summary>
public sealed class CatalogStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IReadOnlyList<ProductDto> _products;

    private CatalogStore(IReadOnlyList<ProductDto> products) => _products = products;

    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, "Data", "catalog.json");

    public static CatalogStore FromFile(string path)
    {
        using var stream = File.OpenRead(path);
        var products = JsonSerializer.Deserialize<List<ProductDto>>(stream, JsonOptions)
            ?? throw new InvalidDataException($"Catalog file is empty: {path}");
        return new CatalogStore(products);
    }

    public IReadOnlyList<ProductDto> GetAll() => _products;
}
