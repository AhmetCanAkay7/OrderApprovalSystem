using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.ViewModels;

public class CreateOrderViewModel
{
    public int PartnerID { get; set; }
    public string? Notes { get; set; }
    public List<Partner> AvailablePartners { get; set; } = new();
    public List<Product> AvailableProducts { get; set; } = new();
    public List<OrderItemInput> OrderItems { get; set; } = new();
}

public class OrderItemInput
{
    public int ProductID { get; set; }
    public int Quantity { get; set; }
}
