namespace OrderApprovalSystem.ViewModels;

public class LoginViewModel
{
    public string LoginType { get; set; } = "Employee"; // "Partner" or "Employee"
    public string? PartnerName { get; set; }
    public string? EmployeeEmail { get; set; }
}
