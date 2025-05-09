using MediatR;

namespace ECommercePI.Application.Features.Orders.Notifications;

public record PaymentFailedNotification(string OrderId) : INotification;