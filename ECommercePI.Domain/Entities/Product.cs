namespace ECommercePI.Domain.Entities;

public class Product
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Category { get; set; } = string.Empty;
    public int Stock { get; set; }
}