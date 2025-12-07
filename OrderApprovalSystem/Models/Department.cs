namespace OrderApprovalSystem.Models;

public class Department
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
