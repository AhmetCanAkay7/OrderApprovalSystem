using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.DataAccess;

public class ReportRepository
{
    private readonly SqlHelper _sqlHelper;

    public ReportRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets partner sales performance.
    /// </summary>
    public List<PartnerSalesViewModel> GetPartnerSalesPerformance()
    {
        const string sql = @"
            SELECT 
                p.PartnerID,
                p.PartnerName,
                COUNT(DISTINCT o.OrderID) AS TotalOrders,
                ISNULL(SUM(o.TotalPrice), 0) AS TotalRevenue,
                ISNULL((
                    SELECT TOP 1 c.CategoryName
                    FROM ORDER_ITEM oi
                    INNER JOIN [ORDER] ord ON oi.OrderID = ord.OrderID
                    INNER JOIN PRODUCT pr ON oi.ProductID = pr.ProductID
                    INNER JOIN CATEGORY c ON pr.CategoryID = c.CategoryID
                    WHERE ord.PartnerID = p.PartnerID
                    GROUP BY c.CategoryName
                    ORDER BY SUM(oi.OrderItemPrice * oi.Quantity) DESC
                ), 'N/A') AS TopCategory
            FROM PARTNER p
            LEFT JOIN [ORDER] o ON p.PartnerID = o.PartnerID AND o.Status = 0
            GROUP BY p.PartnerID, p.PartnerName
            ORDER BY TotalRevenue DESC";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<PartnerSalesViewModel>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new PartnerSalesViewModel
            {
                PartnerID = Convert.ToInt32(row["PartnerID"]),
                PartnerName = row["PartnerName"].ToString() ?? string.Empty,
                TotalOrders = Convert.ToInt32(row["TotalOrders"]),
                TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
                TopCategory = row["TopCategory"].ToString() ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// Gets summary statistics for the dashboard.
    /// </summary>
    public DashboardSummary GetDashboardSummary()
    {
        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM [ORDER] WHERE Status = 1) AS PendingOrders,
                (SELECT COUNT(*) FROM [ORDER] WHERE Status = 0) AS CompletedOrders,
                (SELECT ISNULL(SUM(TotalPrice), 0) FROM [ORDER] WHERE Status = 0) AS TotalRevenue,
                (SELECT COUNT(*) FROM PRODUCT) AS TotalProducts,
                (SELECT COUNT(*) FROM PARTNER) AS TotalPartners";

        var dataTable = _sqlHelper.GetDataTable(sql);

        if (dataTable.Rows.Count == 0)
            return new DashboardSummary();

        var row = dataTable.Rows[0];
        return new DashboardSummary
        {
            PendingOrders = Convert.ToInt32(row["PendingOrders"]),
            CompletedOrders = Convert.ToInt32(row["CompletedOrders"]),
            TotalRevenue = Convert.ToDecimal(row["TotalRevenue"]),
            TotalProducts = Convert.ToInt32(row["TotalProducts"]),
            TotalPartners = Convert.ToInt32(row["TotalPartners"])
        };
    }
}

public class DashboardSummary
{
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalPartners { get; set; }
}
