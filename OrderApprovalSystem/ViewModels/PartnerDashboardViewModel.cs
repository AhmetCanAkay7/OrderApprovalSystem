using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.ViewModels;

public class PartnerDashboardViewModel
{
    public Partner Partner { get; set; } = new();
    public List<DashboardViewModel> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalSpent { get; set; }
}
