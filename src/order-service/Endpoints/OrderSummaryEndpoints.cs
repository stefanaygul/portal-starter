using Microsoft.EntityFrameworkCore;
using SahaPortal.OrderService.Data;
using SahaPortal.OrderService.Services;

namespace SahaPortal.OrderService.Endpoints;

public static class OrderSummaryEndpoints
{
    public static IEndpointRouteBuilder MapOrderSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/summary", SummaryAsync).WithName("GetOrderSummary");
        return app;
    }

    private static async Task<IResult> SummaryAsync(OrdersDbContext db)
    {
        var orders = await db.Orders.AsNoTracking().ToListAsync();
        var products = OrderSummaryCalculator.Summarize(orders);

        return Results.Ok(new
        {
            totalOrders = orders.Count,
            busiest = OrderSummaryCalculator.Busiest(products)?.ProductId,
            products,
        });
    }
}
