using ECommercePI.Application.Features.Orders.Notifications;
using FluentValidation.TestHelper;

namespace ECommercePI.Tests.Unit.Features.Orders.Notifications;

public class PaymentFailedNotificationValidatorTests
{
    [Fact]
    public void PaymentFailedNotification_Should_Have_Error_When_OrderId_Empty()
    {
        var validator = new PaymentFailedNotificationValidator();
        var notification = new PaymentFailedNotification("");

        var result = validator.TestValidate(notification);
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }

    [Fact]
    public void PaymentFailedNotification_Should_Have_Error_When_OrderId_TooLong()
    {
        var validator = new PaymentFailedNotificationValidator();
        var longId = new string('a', 51);
        var notification = new PaymentFailedNotification(longId);

        var result = validator.TestValidate(notification);
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId must be between 1 and 50 characters.");
    }

    [Fact]
    public void PaymentFailedNotification_Should_Not_Have_Error_When_Valid()
    {
        var validator = new PaymentFailedNotificationValidator();
        var notification = new PaymentFailedNotification("order-1");

        var result = validator.TestValidate(notification);
        result.ShouldNotHaveAnyValidationErrors();
    }
}