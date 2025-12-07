namespace OrderApprovalSystem.ViewModels;

public class PartnerSalesViewModel
{
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string TopCategory { get; set; } = string.Empty;
}
