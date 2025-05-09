using ECommercePI.Application.Features.Orders.Notifications;
using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommercePI.Tests.Unit.Features.Orders.Notifications;

public class PaymentFailedNotificationHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IBalanceService> _balanceServiceMock = new();
    private readonly Mock<ILogger<PaymentFailedNotificationHandler>> _loggerMock = new();

    private PaymentFailedNotificationHandler CreateHandler() => new(
        _orderRepoMock.Object,
        _balanceServiceMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task Handle_ShouldUpdateOrder_WhenCancellationSucceeds()
    {
        var notification = new PaymentFailedNotification("order-1");
        var order = new Order { OrderId = "order-1" };
        var cancelTime = DateTime.UtcNow;

        _orderRepoMock.Setup(r => r.GetByIdAsync("order-1")).ReturnsAsync(order);

        _balanceServiceMock.Setup(s => s.CancelPayment("order-1"))
            .ReturnsAsync(BalanceServiceResponse<OrderResult>.Ok(
                new OrderResult
                {
                    Order = new OrderInfo
                    {
                        Status = "cancelled",
                        CancelledAt = cancelTime
                    }
                }, "Cancelled"));

        var handler = CreateHandler();
        await handler.Handle(notification, default);

        order.Status.Should().Be("cancelled");
        order.CancelledAt.Should().Be(cancelTime);

        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdateOrder_WhenCancellationFails()
    {
        var notification = new PaymentFailedNotification("order-2");
        var order = new Order { OrderId = "order-2" };

        _orderRepoMock.Setup(r => r.GetByIdAsync("order-2")).ReturnsAsync(order);

        _balanceServiceMock.Setup(s => s.CancelPayment("order-2"))
            .ReturnsAsync(BalanceServiceResponse<OrderResult>.Fail("Cancellation failed"));

        var handler = CreateHandler();
        await handler.Handle(notification, default);

        order.Status.Should().BeNull();
        order.CancelledAt.Should().BeNull();

        _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}