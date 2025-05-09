using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommercePI.Application.Features.Orders.Notifications;

public class PaymentFailedNotificationHandler(
    IOrderRepository orderRepo,
    IBalanceService balanceService,
    ILogger<PaymentFailedNotificationHandler> logger)
    : INotificationHandler<PaymentFailedNotification>
{
    public async Task Handle(PaymentFailedNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment failed for Order {OrderId}. Attempting to cancel...", notification.OrderId);

        var result = await balanceService.CancelPayment(notification.OrderId);
        var orderToCancel = await orderRepo.GetByIdAsync(notification.OrderId);

        if (result.Success && result.Data?.Order is not null)
        {
            var cancelledOrder = result.Data.Order;
            orderToCancel.Status = cancelledOrder.Status;
            orderToCancel.CancelledAt = cancelledOrder.CancelledAt;
            await orderRepo.UpdateAsync(orderToCancel);

            logger.LogInformation("Order {OrderId} successfully cancelled after payment failure", notification.OrderId);
        }
        else
        {
            logger.LogError("Failed to cancel order {OrderId}. Reason: {Message}", notification.OrderId,
                result.Message);
        }
    }
}