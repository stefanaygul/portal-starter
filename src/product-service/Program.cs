using System.Text.Encodings.Web;
using System.Text.Unicode;
using SahaPortal.ProductService.Catalog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(_ => CatalogStore.FromFile(CatalogStore.DefaultPath));

// Write Turkish characters as-is instead of \uXXXX escapes.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All));

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "product-service v1"));
app.MapHealthChecks("/health");

app.MapGet("/api/products", (CatalogStore catalog) => catalog.GetAll())
    .WithName("GetProducts");

app.Run();

public partial class Program;
