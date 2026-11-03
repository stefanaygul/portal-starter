using Microsoft.EntityFrameworkCore;
using Saha.Common.Contracts;
using SahaPortal.OrderService.Data;

namespace SahaPortal.OrderService.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", CreateAsync).WithName("CreateOrder");
        app.MapGet("/api/orders", ListAsync).WithName("GetOrders");
        return app;
    }

    private static async Task<IResult> CreateAsync(CreateOrderRequest request, OrdersDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId))
        {
            return Results.BadRequest(new { error = "Ekipman kodu boş olamaz." });
        }

        var order = new Order
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            CreatedAt = DateTime.UtcNow,
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return Results.Created($"/api/orders/{order.Id}", ToDto(order));
    }

    private static async Task<IResult> ListAsync(OrdersDbContext db)
    {
        var orders = await db.Orders.AsNoTracking().OrderBy(o => o.Id).ToListAsync();
        return Results.Ok(orders.Select(ToDto));
    }

    // SQLite does not keep DateTimeKind; timestamps are always stored as UTC.
    private static OrderDto ToDto(Order order) =>
        new(order.Id, order.ProductId, order.Quantity, DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc));
}
