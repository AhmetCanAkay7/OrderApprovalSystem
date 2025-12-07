namespace OrderApprovalSystem.Models;

public class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Unit { get; set; }
    public string? ProductCode { get; set; }
    public int MinDeliveryTime { get; set; } = 1;
    public string? Color { get; set; }
    public int CategoryID { get; set; }

    // Navigation property for display
    public string? CategoryName { get; set; }
}
