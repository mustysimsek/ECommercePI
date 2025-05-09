using ECommercePI.Domain.Common;
using MediatR;

namespace ECommercePI.Application.Features.Orders.Command;

public record CreateOrderCommand(List<CreateOrderItem> Items) : IRequest<BalanceServiceResponse<PreOrderResult>>;

public record CreateOrderItem(string ProductId, int Quantity);