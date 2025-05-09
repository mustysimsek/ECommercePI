using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommercePI.Application.Features.Products.Queries;

public class GetProductsQueryHandler(IBalanceService balanceService, ILogger<GetProductsQueryHandler> logger)
    : IRequestHandler<GetProductsQuery, List<Product>>
{
    public async Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product list from Balance API...");

        var response = await balanceService.GetProducts();

        if (!response.Success)
            logger.LogWarning("Failed to fetch products. Reason: {Message}", response.Message);
        else
            logger.LogInformation("Retrieved {Count} product(s) from Balance API", response.Data?.Count ?? 0);

        return response.Data ?? new List<Product>();
    }
}