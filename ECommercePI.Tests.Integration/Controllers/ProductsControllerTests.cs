using System.Net;
using System.Net.Http.Json;
using ECommercePI.WebAPI;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ECommercePI.Tests.Integration.Controllers;

public class ProductsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_ShouldReturnOk_WithProducts()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<object>>();
        products.Should().NotBeNull();
    }
}