using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using ECommercePI.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommercePI.Application.Features.Orders.Command;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepo,
    IBalanceService balanceService,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, BalanceServiceResponse<PreOrderResult>>
{
    public async Task<BalanceServiceResponse<PreOrderResult>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid().ToString();
        var orderItems = new List<Domain.Entities.OrderItem>();
        decimal totalAmount = 0;

        logger.LogInformation("Creating order {OrderId} with {ItemCount} item(s)", orderId, request.Items.Count);

        foreach (var item in request.Items)
        {
            var product = await balanceService.GetProductByIdAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                logger.LogWarning("Product {ProductId} is not in stock or available", item.ProductId);
                return BalanceServiceResponse<PreOrderResult>.Fail(
                    $"Product {item.ProductId} is not in stock or available");
            }

            var orderItem = new Domain.Entities.OrderItem
            {
                OrderItemId = Guid.NewGuid().ToString(),
                ProductId = product.Id,
                Quantity = item.Quantity,
                Price = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += item.Quantity * product.Price;
        }

        logger.LogInformation("Reserving funds for OrderId {OrderId}, TotalAmount: {TotalAmount}", orderId,
            totalAmount);

        var result = await balanceService.ReserveFunds(orderId, totalAmount);

        if (!result.Success || result.Data?.PreOrder == null)
        {
            logger.LogError("Failed to reserve funds for OrderId {OrderId}: {Message}", orderId, result.Message);
            return BalanceServiceResponse<PreOrderResult>.Fail(result.Message ?? "Failed to get balance information");
        }

        if (result.Data.UpdatedBalance.AvailableBalance < totalAmount)
        {
            logger.LogWarning(
                "Not enough balance for OrderId {OrderId}. Required: {Required}, Available: {Available}",
                orderId, totalAmount, result.Data.UpdatedBalance.AvailableBalance);

            return BalanceServiceResponse<PreOrderResult>.Fail(result.Message ??
                                                               "There is no available balance for pre-order");
        }

        var order = new Order
        {
            OrderId = result.Data.PreOrder.OrderId,
            Status = result.Data.PreOrder.Status,
            Items = orderItems,
            Amount = result.Data.PreOrder.Amount,
            CreatedAt = result.Data.PreOrder.Timestamp
        };

        await orderRepo.SaveAsync(order);

        logger.LogInformation("Order {OrderId} successfully created", orderId);

        return BalanceServiceResponse<PreOrderResult>.Ok(result.Data, "Order created successfully.");
    }
}