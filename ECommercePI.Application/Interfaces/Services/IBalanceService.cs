using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;

namespace ECommercePI.Application.Interfaces.Services;

public interface IBalanceService
{
    Task<BalanceServiceResponse<PreOrderResult>> ReserveFunds(string orderId, decimal amount);
    Task<BalanceServiceResponse<OrderResult>> CompletePayment(string orderId);
    Task<BalanceServiceResponse<OrderResult>> CancelPayment(string orderId);

    Task<BalanceServiceResponse<BalanceInfo>> GetBalance();
    Task<BalanceServiceResponse<List<Product>>> GetProducts();
    Task<Product?> GetProductByIdAsync(string productId);
}