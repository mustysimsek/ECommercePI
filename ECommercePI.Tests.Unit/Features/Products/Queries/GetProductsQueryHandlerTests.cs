using ECommercePI.Application.Features.Products.Queries;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommercePI.Tests.Unit.Features.Products.Queries;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IBalanceService> _balanceServiceMock = new();
    private readonly Mock<ILogger<GetProductsQueryHandler>> _loggerMock = new();

    private GetProductsQueryHandler CreateHandler() => new(
        _balanceServiceMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task Handle_ShouldReturnProducts_WhenSuccessful()
    {
        var products = new List<Product> { new() { Id = "p1" }, new() { Id = "p2" } };

        _balanceServiceMock.Setup(s => s.GetProducts())
            .ReturnsAsync(BalanceServiceResponse<List<Product>>.Ok(products));

        var handler = CreateHandler();
        var result = await handler.Handle(new GetProductsQuery(), default);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == "p1");
        result.Should().Contain(p => p.Id == "p2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenApiFails()
    {
        _balanceServiceMock.Setup(s => s.GetProducts())
            .ReturnsAsync(BalanceServiceResponse<List<Product>>.Fail("error"));

        var handler = CreateHandler();
        var result = await handler.Handle(new GetProductsQuery(), default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}