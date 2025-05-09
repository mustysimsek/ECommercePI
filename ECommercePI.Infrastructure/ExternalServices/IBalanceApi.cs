using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using ECommercePI.Infrastructure.ExternalServices.Models;
using Refit;

namespace ECommercePI.Infrastructure.ExternalServices;

public interface IBalanceApi
{
    [Post("/api/balance/preorder")]
    Task<ApiResponse<BalanceServiceResponse<PreOrderResult>>> ReserveFunds([Body] PreorderRequest request);

    [Post("/api/balance/complete")]
    Task<ApiResponse<BalanceServiceResponse<OrderResult>>> CompletePayment([Body] CompleteRequest request);

    [Post("/api/balance/cancel")]
    Task<ApiResponse<BalanceServiceResponse<OrderResult>>> CancelPayment([Body] CancelRequest request);

    [Get("/api/balance")]
    Task<ApiResponse<BalanceServiceResponse<BalanceInfo>>> GetBalance();

    [Get("/api/products")]
    Task<ApiResponse<BalanceServiceResponse<List<Product>>>> GetProducts();
}