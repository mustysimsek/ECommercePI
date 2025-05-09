namespace ECommercePI.Domain.Common;

public class OrderResult
{
    public OrderInfo? Order { get; set; }
    public BalanceInfo? UpdatedBalance { get; set; }
}