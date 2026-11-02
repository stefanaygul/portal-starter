using Microsoft.EntityFrameworkCore;

namespace SahaPortal.OrderService.Data;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
}
