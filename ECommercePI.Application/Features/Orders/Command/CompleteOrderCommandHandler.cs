using ECommercePI.Application.Features.Orders.Notifications;
using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommercePI.Application.Features.Orders.Command;

public class CompleteOrderCommandHandler(
    IOrderRepository orderRepo,
    IBalanceService balanceService,
    IMediator mediator,
    ILogger<CompleteOrderCommandHandler> logger)
    : IRequestHandler<CompleteOrderCommand, BalanceServiceResponse<OrderResult>>
{
    public async Task<BalanceServiceResponse<OrderResult>> Handle(CompleteOrderCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting payment completion for OrderId: {OrderId}", request.OrderId);

        var order = await orderRepo.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            logger.LogWarning("Order with ID {OrderId} not found.", request.OrderId);
            return BalanceServiceResponse<OrderResult>.Fail("Order not found.");
        }

        var result = await balanceService.CompletePayment(request.OrderId);
        if (!result.Success)
        {
            logger.LogWarning("Payment failed for OrderId: {OrderId}. Reason: {Reason}",
                request.OrderId, result.Message);
            await mediator.Publish(new PaymentFailedNotification(request.OrderId), cancellationToken);
            return BalanceServiceResponse<OrderResult>.Fail(result.Message ?? "Failed to payment");
        }

        if (result.Data?.Order is not null)
        {
            var completedOrder = result.Data.Order;
            order.Status = completedOrder.Status;
            order.CompletedAt = completedOrder.CompletedAt;
            await orderRepo.UpdateAsync(order);

            logger.LogInformation("Order {OrderId} marked as completed at {CompletedAt}",
                order.OrderId, order.CompletedAt);
        }

        return BalanceServiceResponse<OrderResult>.Ok(result.Data, "Payment made successfully.");
    }
}