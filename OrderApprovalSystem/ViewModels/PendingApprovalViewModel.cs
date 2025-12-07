namespace OrderApprovalSystem.ViewModels;

public class PendingApprovalViewModel
{
    public int OrderID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int EmployeeID { get; set; }
    public string OrdererFullName { get; set; } = string.Empty;
}
