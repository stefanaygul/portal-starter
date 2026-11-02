using Microsoft.EntityFrameworkCore;
using SahaPortal.OrderService.Data;
using SahaPortal.OrderService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// The connection string is read when the context is first used, so an
// environment variable (ConnectionStrings__Orders) can override appsettings.json.
builder.Services.AddDbContext<OrdersDbContext>((services, options) =>
    options.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Orders")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureCreated();
}

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "order-service v1"));
app.MapHealthChecks("/health");
app.MapOrderEndpoints();

app.Run();

public partial class Program;
