namespace OrderApprovalSystem.Models;

public class Stock
{
    public int StockID { get; set; }
    public int ProductID { get; set; }
    public int WarehouseID { get; set; }
    public int Quantity { get; set; }
    public DateTime LastUpdateDate { get; set; }

    // Navigation properties for display
    public string? ProductName { get; set; }
    public string? WarehouseName { get; set; }
    public decimal? Price { get; set; }
    public decimal TotalValue => Quantity * (Price ?? 0);
}
