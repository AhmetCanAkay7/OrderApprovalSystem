namespace OrderApprovalSystem.Models;

public class OrderApprover
{
    public int OrderID { get; set; }
    public int EmployeeID { get; set; }

    // Navigation properties for display
    public string? EmployeeName { get; set; }
    public string? RoleName { get; set; }
}
