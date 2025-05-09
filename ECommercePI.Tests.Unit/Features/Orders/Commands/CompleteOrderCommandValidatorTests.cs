using ECommercePI.Application.Features.Orders.Command;
using FluentValidation.TestHelper;
using Xunit;

namespace ECommercePI.Tests.Unit.Features.Orders.Commands;

public class CompleteOrderCommandValidatorTests
{
    private readonly CompleteOrderCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_OrderId_Is_Empty()
    {
        var command = new CompleteOrderCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OrderId)
            .WithErrorMessage("OrderId is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OrderId_Is_Provided()
    {
        var command = new CompleteOrderCommand("order-123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.OrderId);
    }
}