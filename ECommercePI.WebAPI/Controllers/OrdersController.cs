using ECommercePI.Application.Features.Orders.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new order and reserve balance.
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Finalize an order and complete payment.
    /// </summary>
    [HttpPost("{orderId}/complete")]
    public async Task<IActionResult> CompleteOrder(string orderId)
    {
        var result = await mediator.Send(new CompleteOrderCommand(orderId));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}