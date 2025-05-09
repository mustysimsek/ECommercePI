namespace ECommercePI.Domain.Common;

public class BalanceInfo
{
    public string? UserId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal BlockedBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime LastUpdated { get; set; }
}