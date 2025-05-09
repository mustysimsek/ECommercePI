using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using ECommercePI.Infrastructure.ExternalServices;
using ECommercePI.Infrastructure.ExternalServices.Helpers;
using ECommercePI.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Refit;

namespace ECommercePI.Infrastructure.Services;

public class BalanceService : IBalanceService
{
    private readonly IBalanceApi _api;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<BalanceService> _logger;
    private const string ProductsCacheKey = "products";

    public BalanceService(IBalanceApi api, IMemoryCache memoryCache, ILogger<BalanceService> logger)
    {
        _api = api;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<BalanceServiceResponse<PreOrderResult>> ReserveFunds(string orderId, decimal amount)
    {
        var response = await _api.ReserveFunds(new PreorderRequest { OrderId = orderId, Amount = amount });
        return ParseResponse(response, "ReserveFunds", _logger, orderId);
    }

    public async Task<BalanceServiceResponse<OrderResult>> CompletePayment(string orderId)
    {
        var response = await _api.CompletePayment(new CompleteRequest { OrderId = orderId });
        return ParseResponse(response, "CompletePayment", _logger, orderId);
    }

    public async Task<BalanceServiceResponse<OrderResult>> CancelPayment(string orderId)
    {
        var response = await _api.CancelPayment(new CancelRequest { OrderId = orderId });
        return ParseResponse(response, "CancelPayment", _logger, orderId);
    }

    public async Task<BalanceServiceResponse<BalanceInfo>> GetBalance()
    {
        var response = await _api.GetBalance();
        return ParseResponse(response, "GetBalance", _logger);
    }

    public async Task<BalanceServiceResponse<List<Product>>> GetProducts()
    {
        if (_memoryCache.TryGetValue(ProductsCacheKey, out BalanceServiceResponse<List<Product>>? cachedResult) &&
            cachedResult?.Success == true &&
            cachedResult.Data?.Count > 0)
        {
            return cachedResult;
        }

        var response = await _api.GetProducts();
        var result = ParseResponse(response, "GetProducts", _logger);

        if (result.Success && result.Data?.Count > 0)
        {
            _memoryCache.Set(ProductsCacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    public async Task<Product?> GetProductByIdAsync(string productId)
    {
        var result = await GetProducts();
        return result.Data?.FirstOrDefault(p => p.Id == productId);
    }

    // ✔️ Centralized response parser with Serilog-enabled logging
    private static BalanceServiceResponse<T> ParseResponse<T>(
        Refit.ApiResponse<BalanceServiceResponse<T>> response,
        string operation,
        ILogger logger,
        string? referenceId = null)
    {
        if (response.Content != null && response.Content.Data != null)
        {
            return BalanceServiceResponse<T>.Ok(response.Content.Data, response.Content.Message);
        }

        var errorMessage = ApiErrorParser.ExtractMessage(response.Error?.Content)
                           ?? response.Content?.Message
                           ?? "Unexpected error";

        logger.LogWarning("API Call Failed | Operation: {Operation} | Ref: {Ref} | Message: {Message}",
            operation, referenceId ?? "-", errorMessage);

        return BalanceServiceResponse<T>.Fail(errorMessage);
    }
}