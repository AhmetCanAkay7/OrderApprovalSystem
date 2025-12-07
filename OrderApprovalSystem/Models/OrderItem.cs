namespace OrderApprovalSystem.Models;

public class OrderItem
{
    public int OrderItemID { get; set; }
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public string? OrderItemName { get; set; }
    public string? Unit { get; set; }
    public decimal OrderItemPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }
    public string? Currency { get; set; }

    // Navigation properties for display
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }

    // Calculated property
    public decimal LineTotal => OrderItemPrice * Quantity;
}
