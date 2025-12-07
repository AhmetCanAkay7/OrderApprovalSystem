namespace OrderApprovalSystem.Models;

public class Employee
{
    public int EmployeeID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime StartDate { get; set; }
    public int DepartmentID { get; set; }
    public int? RoleID { get; set; }
    public int? ManagerID { get; set; }

    // Navigation properties (for view display)
    public string? DepartmentName { get; set; }
    public string? ManagerName { get; set; }
    public string? RoleName { get; set; }
    
    public string FullName => $"{Name} {Surname}";
}
