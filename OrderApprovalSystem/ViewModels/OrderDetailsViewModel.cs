using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.ViewModels;

public class OrderDetailsViewModel
{
    public Order Order { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<OrderApprover> Approvers { get; set; } = new();
    
    public bool CanUserApprove { get; set; }
    public int? CurrentUserEmployeeID { get; set; }
    public string? CurrentUserRole { get; set; }
}
