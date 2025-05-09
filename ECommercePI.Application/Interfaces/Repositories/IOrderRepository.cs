using ECommercePI.Domain.Entities;

namespace ECommercePI.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string orderId);
    Task SaveAsync(Order order);
    Task UpdateAsync(Order order);
}