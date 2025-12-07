namespace OrderApprovalSystem.Models;

public class Warehouse
{
    public int WarehouseID { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}
