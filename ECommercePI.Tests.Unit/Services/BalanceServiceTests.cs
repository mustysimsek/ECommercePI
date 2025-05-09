using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommercePI.Domain.Common;
using ECommercePI.Domain.Entities;
using ECommercePI.Infrastructure.ExternalServices;
using ECommercePI.Infrastructure.ExternalServices.Models;
using ECommercePI.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using Xunit;

public class BalanceServiceTests
{
    private readonly Mock<IBalanceApi> _apiMock = new();
    private readonly Mock<ILogger<BalanceService>> _loggerMock = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private BalanceService CreateService() => new(_apiMock.Object, _cache, _loggerMock.Object);

    private ApiResponse<T> CreateSuccessResponse<T>(T content) where T : class
    {
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        return new ApiResponse<T>(responseMessage, content, new RefitSettings(), null);
    }

    [Fact]
    public async Task ReserveFunds_ShouldReturnSuccess()
    {
        var response = CreateSuccessResponse(new BalanceServiceResponse<PreOrderResult>
        {
            Success = true,
            Message = "ok",
            Data = new PreOrderResult { PreOrder = new OrderInfo(), UpdatedBalance = new BalanceInfo() }
        });

        _apiMock.Setup(a => a.ReserveFunds(It.IsAny<PreorderRequest>())).ReturnsAsync(response);

        var result = await CreateService().ReserveFunds("order-1", 100);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CompletePayment_ShouldReturnFailure_WithErrorJson()
    {
        var errorJson = JsonSerializer.Serialize(new ErrorResponse { Message = "error msg" });

        var error = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "http://localhost"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorJson)
            },
            new RefitSettings());

        var response = new ApiResponse<BalanceServiceResponse<OrderResult>>(
            new HttpResponseMessage(HttpStatusCode.BadRequest), null, new RefitSettings(), error);

        _apiMock.Setup(a => a.CompletePayment(It.IsAny<CompleteRequest>())).ReturnsAsync(response);

        var result = await CreateService().CompletePayment("order-2");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("error msg");
    }

    [Fact]
    public async Task CancelPayment_ShouldReturnFailure_WithMalformedError()
    {
        var error = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "http://localhost"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("not json")
            },
            new RefitSettings());

        var response = new ApiResponse<BalanceServiceResponse<OrderResult>>(
            new HttpResponseMessage(HttpStatusCode.BadRequest), null, new RefitSettings(), error);

        _apiMock.Setup(a => a.CancelPayment(It.IsAny<CancelRequest>())).ReturnsAsync(response);

        var result = await CreateService().CancelPayment("order-3");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("error");
    }

    [Fact]
    public async Task GetBalance_ShouldReturnUnexpectedError_WhenAllNull()
    {
        var response = new ApiResponse<BalanceServiceResponse<BalanceInfo>>(
            new HttpResponseMessage(HttpStatusCode.InternalServerError), null, new RefitSettings(), null);

        _apiMock.Setup(a => a.GetBalance()).ReturnsAsync(response);

        var result = await CreateService().GetBalance();

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Unexpected error");
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFromCache()
    {
        var cached = BalanceServiceResponse<List<Product>>.Ok(
            new List<Product> { new() { Id = "p1" } }, "cached");
        _cache.Set("products", cached);

        var result = await CreateService().GetProducts();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProducts_ShouldCacheSuccessfulResponse()
    {
        var products = new List<Product> { new() { Id = "p1" } };
        var response = CreateSuccessResponse(new BalanceServiceResponse<List<Product>>
        {
            Success = true,
            Message = "ok",
            Data = products
        });

        _apiMock.Setup(a => a.GetProducts()).ReturnsAsync(response);

        var result = await CreateService().GetProducts();

        result.Success.Should().BeTrue();
        _cache.TryGetValue("products", out BalanceServiceResponse<List<Product>>? cached).Should().BeTrue();
        cached?.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnMatchedProduct()
    {
        var product = new Product { Id = "match" };
        var response = CreateSuccessResponse(new BalanceServiceResponse<List<Product>>
        {
            Success = true,
            Data = new List<Product> { product }
        });

        _apiMock.Setup(a => a.GetProducts()).ReturnsAsync(response);

        var result = await CreateService().GetProductByIdAsync("match");

        result.Should().NotBeNull();
        result!.Id.Should().Be("match");
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var response = CreateSuccessResponse(new BalanceServiceResponse<List<Product>>
        {
            Success = true,
            Data = new List<Product>()
        });

        _apiMock.Setup(a => a.GetProducts()).ReturnsAsync(response);

        var result = await CreateService().GetProductByIdAsync("not-found");

        result.Should().BeNull();
    }
}
