namespace OrderApprovalSystem.ViewModels;

public class DashboardViewModel
{
    public int OrderID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string CurrentStepText { get; set; } = string.Empty;
    public string OrdererFullName { get; set; } = string.Empty;
    public int Status { get; set; }
    public int CurrentStepValue { get; set; }
}
