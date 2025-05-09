using ECommercePI.Application.Features.Orders.Command;
using ECommercePI.Application.Features.Orders.Notifications;
using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommercePI.Tests.Unit.Features.Orders.Commands;

public class CompleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IBalanceService> _balanceServiceMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<ILogger<CompleteOrderCommandHandler>> _loggerMock = new();

    private CompleteOrderCommandHandler CreateHandler()
    {
        return new CompleteOrderCommandHandler(
            _orderRepoMock.Object,
            _balanceServiceMock.Object,
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOrderNotFound()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync("order-1")).ReturnsAsync((Order?)null);

        var handler = CreateHandler();
        var command = new CompleteOrderCommand("order-1");

        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Order not found.");
    }

    [Fact]
    public async Task Handle_ShouldFail_AndPublishNotification_WhenPaymentFails()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync("order-2")).ReturnsAsync(new Order { OrderId = "order-2" });

        _balanceServiceMock.Setup(s => s.CompletePayment("order-2"))
            .ReturnsAsync(BalanceServiceResponse<OrderResult>.Fail("Payment failed."));

        var handler = CreateHandler();
        var command = new CompleteOrderCommand("order-2");

        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payment failed.");

        _mediatorMock.Verify(m => m.Publish(It.IsAny<PaymentFailedNotification>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOrder_WhenPaymentSuccessfulWithOrderData()
    {
        var order = new Order { OrderId = "order-3" };
        var completedAt = DateTime.UtcNow;

        _orderRepoMock.Setup(r => r.GetByIdAsync("order-3")).ReturnsAsync(order);

        _balanceServiceMock.Setup(s => s.CompletePayment("order-3"))
            .ReturnsAsync(BalanceServiceResponse<OrderResult>.Ok(
                new OrderResult
                {
                    Order = new OrderInfo
                    {
                        OrderId = "order-3",
                        Status = "completed",
                        CompletedAt = completedAt
                    },
                    UpdatedBalance = null
                },
                "Success"));

        var handler = CreateHandler();
        var command = new CompleteOrderCommand("order-3");

        var result = await handler.Handle(command, default);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Order.Should().NotBeNull();
        result.Data.Order?.Status.Should().Be("completed");

        order.Status.Should().Be("completed");
        order.CompletedAt.Should().Be(completedAt);

        _orderRepoMock.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WithoutUpdate_WhenPaymentSuccess_ButNoOrderReturned()
    {
        var order = new Order { OrderId = "order-4" };
        _orderRepoMock.Setup(r => r.GetByIdAsync("order-4")).ReturnsAsync(order);

        _balanceServiceMock.Setup(s => s.CompletePayment("order-4"))
            .ReturnsAsync(BalanceServiceResponse<OrderResult>.Ok(
                new OrderResult
                {
                    Order = null,
                    UpdatedBalance = null
                },
                "No data"));

        var handler = CreateHandler();
        var command = new CompleteOrderCommand("order-4");

        var result = await handler.Handle(command, default);

        result.Success.Should().BeTrue();
        result.Data?.Order.Should().BeNull();

        order.Status.Should().BeNull();
        order.CompletedAt.Should().BeNull();

        _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}