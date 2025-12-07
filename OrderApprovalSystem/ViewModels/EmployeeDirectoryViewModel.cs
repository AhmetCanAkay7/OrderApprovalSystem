namespace OrderApprovalSystem.ViewModels;

public class EmployeeDirectoryViewModel
{
    public int EmployeeID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? RoleName { get; set; }
    public DateTime StartDate { get; set; }
}
