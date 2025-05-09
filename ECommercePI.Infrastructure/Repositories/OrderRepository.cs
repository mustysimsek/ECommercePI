using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePI.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(string orderId) =>
        await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task SaveAsync(Order order)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        context.Orders.Update(order);
        await context.SaveChangesAsync();
    }
}