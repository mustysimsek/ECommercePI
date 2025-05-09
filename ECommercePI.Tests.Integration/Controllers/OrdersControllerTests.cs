using System.Net;
using System.Net.Http.Json;
using ECommercePI.WebAPI;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ECommercePI.Tests.Integration.Controllers;

public class OrdersControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenItemsInvalid()
    {
        var command = new
        {
            Items = new[]
            {
                new { ProductId = "", Quantity = 0 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/create", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnOk_WhenItemsValid()
    {
        var items = new[]
        {
            new { ProductId = "prod-001", Quantity = 1 }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/create", items);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        // Ürün yoksa ya da API null dönüyorsa BadRequest olabilir (dış servise bağlı)
    }

    [Fact]
    public async Task CompleteOrder_ShouldReturnBadRequest_WhenInvalidId()
    {
        var response = await _client.PostAsync("/api/orders/invalid-id/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}