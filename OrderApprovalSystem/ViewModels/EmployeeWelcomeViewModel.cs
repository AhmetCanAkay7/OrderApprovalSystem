using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.ViewModels;

public class EmployeeWelcomeViewModel
{
    public Employee Employee { get; set; } = new();
    public string WelcomeMessage { get; set; } = string.Empty;
    public DateTime CurrentTime { get; set; } = DateTime.Now;
}
