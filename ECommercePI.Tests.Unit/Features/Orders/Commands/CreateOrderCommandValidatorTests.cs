using ECommercePI.Application.Features.Orders.Command;
using FluentValidation.TestHelper;

namespace ECommercePI.Tests.Unit.Features.Orders.Commands;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void CreateOrderCommand_Should_Have_Error_When_Items_Empty()
    {
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(new List<CreateOrderItem>());

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void CreateOrderCommand_Should_Have_Errors_For_Invalid_Items()
    {
        var validator = new CreateOrderCommandValidator();
        var items = new List<CreateOrderItem> { new("", 0) };
        var command = new CreateOrderCommand(items);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
            .WithErrorMessage("ProductId is required.");

        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
            .WithErrorMessage("ProductId cannot be empty or whitespace.");

        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }


    [Fact]
    public void CreateOrderCommand_Should_Not_Have_Errors_For_Valid_Input()
    {
        var validator = new CreateOrderCommandValidator();
        var items = new List<CreateOrderItem> { new("p1", 2) };
        var command = new CreateOrderCommand(items);

        var result = validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}