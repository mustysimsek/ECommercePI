using ECommercePI.Application.Features.Orders.Command;
using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommercePI.Tests.Unit.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IBalanceService> _balanceServiceMock = new();
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock = new();

    private CreateOrderCommandHandler CreateHandler() => new(
        _orderRepoMock.Object,
        _balanceServiceMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task Handle_ShouldFail_WhenProductIsUnavailable()
    {
        var command = new CreateOrderCommand(new List<CreateOrderItem>
        {
            new CreateOrderItem("p1", 2)
        });

        _balanceServiceMock.Setup(s => s.GetProductByIdAsync("p1"))
            .ReturnsAsync((Product?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product p1 is not in stock or available");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStockIsNotEnough()
    {
        var command = new CreateOrderCommand(new List<CreateOrderItem>
        {
            new CreateOrderItem("p1", 5)
        });

        _balanceServiceMock.Setup(s => s.GetProductByIdAsync("p1"))
            .ReturnsAsync(new Product { Id = "p1", Stock = 2, Price = 100 });

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Product p1 is not in stock or available");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBalanceReservationFails()
    {
        var command = new CreateOrderCommand(new List<CreateOrderItem>
        {
            new CreateOrderItem("p1", 1)
        });

        _balanceServiceMock.Setup(s => s.GetProductByIdAsync("p1"))
            .ReturnsAsync(new Product { Id = "p1", Stock = 10, Price = 50 });

        _balanceServiceMock.Setup(s => s.ReserveFunds(It.IsAny<string>(), 50))
            .ReturnsAsync(BalanceServiceResponse<PreOrderResult>.Fail("Reserve failed"));

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Reserve failed");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAvailableBalanceIsTooLow()
    {
        var command = new CreateOrderCommand(new List<CreateOrderItem>
        {
            new CreateOrderItem("p1",2)
        });

        _balanceServiceMock.Setup(s => s.GetProductByIdAsync("p1"))
            .ReturnsAsync(new Product { Id = "p1", Stock = 5, Price = 100 });

        _balanceServiceMock.Setup(s => s.ReserveFunds(It.IsAny<string>(), 200))
            .ReturnsAsync(BalanceServiceResponse<PreOrderResult>.Ok(
                new PreOrderResult
                {
                    PreOrder = new OrderInfo
                    {
                        OrderId = "oid",
                        Status = "blocked",
                        Amount = 200,
                        Timestamp = DateTime.UtcNow
                    },
                    UpdatedBalance = new BalanceInfo { AvailableBalance = 150 }
                }, "not enough balance"));

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not enough balance");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenAllValid()
    {
        var command = new CreateOrderCommand(new List<CreateOrderItem>
        {
            new CreateOrderItem("p1",2)
        });

        var orderId = "generated-id";
        var orderInfo = new OrderInfo
        {
            OrderId = orderId,
            Amount = 200,
            Status = "blocked",
            Timestamp = DateTime.UtcNow
        };

        _balanceServiceMock.Setup(s => s.GetProductByIdAsync("p1"))
            .ReturnsAsync(new Product { Id = "p1", Stock = 5, Price = 100 });

        _balanceServiceMock.Setup(s => s.ReserveFunds(It.IsAny<string>(), 200))
            .ReturnsAsync(BalanceServiceResponse<PreOrderResult>.Ok(
                new PreOrderResult
                {
                    PreOrder = orderInfo,
                    UpdatedBalance = new BalanceInfo { AvailableBalance = 500 }
                }, "ok"));

        _orderRepoMock.Setup(r => r.SaveAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.Handle(command, default);

        result.Success.Should().BeTrue();
        result.Data?.PreOrder?.OrderId.Should().Be(orderId);
        _orderRepoMock.Verify(r => r.SaveAsync(It.Is<Order>(o => o.OrderId == orderId)), Times.Once);
    }
}
