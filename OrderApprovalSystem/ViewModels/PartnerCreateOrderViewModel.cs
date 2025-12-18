using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.ViewModels;

public class PartnerCreateOrderViewModel
{
    public int PartnerID { get; set; }
    public string? PartnerName { get; set; }
    public string? Notes { get; set; }
    public string? PaymentTerm { get; set; }
    public string Currency { get; set; } = "EUR";

    public List<Product> AvailableProducts { get; set; } = new();

    // Uses shared OrderItemInput from CreateOrderViewModel
    public List<OrderItemInput> Items { get; set; } = new();
}
