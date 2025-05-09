using ECommercePI.WebAPI.Middlewares;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace ECommercePI.Tests.Integration.Middlewares;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Should_ReturnValidationErrorMessage_When_ValidationExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        var middleware = new ExceptionMiddleware(loggerMock.Object);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var validationException = new ValidationException(new[]
        {
            new ValidationFailure("Field", "Validation failed.")
        });

        // Act
        await middleware.InvokeAsync(context, _ => throw validationException);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().StartWith("application/json");

        responseStream.Position = 0;
        var json = await new StreamReader(responseStream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("success").GetBoolean().Should().BeFalse();
        result.GetProperty("message").GetString().Should().Be("Validation failed.");

        var errors = result.GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("propertyName").GetString().Should().Be("Field");
        errors[0].GetProperty("errorMessage").GetString().Should().Be("Validation failed.");
    }

    [Fact]
    public async Task Should_ReturnGenericErrorMessage_When_OtherExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        var middleware = new ExceptionMiddleware(loggerMock.Object);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        await middleware.InvokeAsync(context, _ => throw new InvalidOperationException("Something went wrong"));

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        responseStream.Position = 0;
        var json = await new StreamReader(responseStream).ReadToEndAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        result.GetProperty("success").GetBoolean().Should().BeFalse();
        result.GetProperty("message").GetString().Should().Be("Something went wrong");
    }
}
