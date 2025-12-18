namespace OrderApprovalSystem.ViewModels;

public class StockCheckResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<InsufficientStockItem> InsufficientItems { get; set; } = new();
}

public class InsufficientStockItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
