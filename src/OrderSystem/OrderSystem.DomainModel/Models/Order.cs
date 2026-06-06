namespace OrderSystem.OrderGen.Models;

public class Order
{
    public long OrderId { get; set; }
    public long UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Amount { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public void CalculateTotal()
    {
        TotalPrice = Amount * UnitPrice;
    }
}