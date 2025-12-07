namespace OrderApprovalSystem.Models;

public class Order
{
    public int OrderID { get; set; }
    public int PartnerID { get; set; }
    public int CurrentStepValue { get; set; } // 0 = Commercial, 1 = Technical, 2 = Paraf, 3 = Ready for completion
    public string? OrdererName { get; set; }
    public string? OrdererSurname { get; set; }
    public int Status { get; set; } // 0 = Completed, 1 = Active/Pending
    public DateTime CreationTime { get; set; }
    public decimal TotalPrice { get; set; }
    public string? PaymentTerm { get; set; }
    public string? Currency { get; set; }
    public string? OrderNote { get; set; }

    // Navigation properties for display
    public string? PartnerName { get; set; }

    public string OrdererFullName => $"{OrdererName} {OrdererSurname}";

    public string StatusText => Status switch
    {
        0 => "Completed",
        1 => "Pending Approval",
        _ => "Unknown"
    };

    public string CurrentStepText => CurrentStepValue switch
    {
        0 => "Commercial Approval",
        1 => "Technical Approval",
        2 => "Paraf Approval",
        3 => "All Approvals Complete",
        _ => "Unknown"
    };
}
