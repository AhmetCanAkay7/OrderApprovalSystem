namespace OrderApprovalSystem.ViewModels;

public class InventoryValuationViewModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
}
