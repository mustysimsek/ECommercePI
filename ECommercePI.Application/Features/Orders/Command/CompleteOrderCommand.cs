using ECommercePI.Domain.Common;
using MediatR;

namespace ECommercePI.Application.Features.Orders.Command;

public record CompleteOrderCommand(string OrderId) : IRequest<BalanceServiceResponse<OrderResult>>;