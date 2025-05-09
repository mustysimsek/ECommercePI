namespace ECommercePI.Infrastructure.ExternalServices.Models;

public class PreorderRequest
{
    public string OrderId { get; set; } = null!;
    public decimal Amount { get; set; }
}