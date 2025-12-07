using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.Models;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.DataAccess;

public class StockRepository
{
    private readonly SqlHelper _sqlHelper;

    public StockRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets inventory valuation by warehouse and product.
    /// </summary>
    public List<InventoryValuationViewModel> GetInventoryValuation()
    {
        const string sql = @"
            SELECT 
                p.ProductID,
                p.ProductName,
                c.CategoryName,
                w.WarehouseName,
                s.Quantity,
                p.Price,
                s.Quantity * p.Price AS TotalValue
            FROM STOCK s
            INNER JOIN PRODUCT p ON s.ProductID = p.ProductID
            INNER JOIN CATEGORY c ON p.CategoryID = c.CategoryID
            INNER JOIN WAREHOUSE w ON s.WarehouseID = w.WarehouseID
            WHERE s.Quantity > 0
            ORDER BY w.WarehouseName, c.CategoryName, p.ProductName";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<InventoryValuationViewModel>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new InventoryValuationViewModel
            {
                ProductID = Convert.ToInt32(row["ProductID"]),
                ProductName = row["ProductName"].ToString() ?? string.Empty,
                CategoryName = row["CategoryName"].ToString() ?? string.Empty,
                WarehouseName = row["WarehouseName"].ToString() ?? string.Empty,
                Quantity = Convert.ToInt32(row["Quantity"]),
                Price = row["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Price"]),
                TotalValue = row["TotalValue"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalValue"])
            });
        }

        return result;
    }

    /// <summary>
    /// Gets all warehouses.
    /// </summary>
    public List<Warehouse> GetAllWarehouses()
    {
        const string sql = @"
            SELECT 
                WarehouseID,
                WarehouseName,
                Country,
                City,
                State,
                ZipCode
            FROM WAREHOUSE
            ORDER BY WarehouseName";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<Warehouse>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new Warehouse
            {
                WarehouseID = Convert.ToInt32(row["WarehouseID"]),
                WarehouseName = row["WarehouseName"].ToString() ?? string.Empty,
                Country = row["Country"] == DBNull.Value ? null : row["Country"].ToString(),
                City = row["City"] == DBNull.Value ? null : row["City"].ToString(),
                State = row["State"] == DBNull.Value ? null : row["State"].ToString(),
                ZipCode = row["ZipCode"] == DBNull.Value ? null : row["ZipCode"].ToString()
            });
        }

        return result;
    }

    /// <summary>
    /// Gets stock for a specific product across all warehouses.
    /// </summary>
    public List<Stock> GetStockByProduct(int productId)
    {
        const string sql = @"
            SELECT 
                s.StockID,
                s.ProductID,
                s.WarehouseID,
                s.Quantity,
                s.LastUpdateDate,
                p.ProductName,
                w.WarehouseName,
                p.Price
            FROM STOCK s
            INNER JOIN PRODUCT p ON s.ProductID = p.ProductID
            INNER JOIN WAREHOUSE w ON s.WarehouseID = w.WarehouseID
            WHERE s.ProductID = @ProductID
            ORDER BY s.Quantity DESC";

        var parameters = new[]
        {
            new SqlParameter("@ProductID", productId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);
        var result = new List<Stock>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(MapRowToStock(row));
        }

        return result;
    }

    /// <summary>
    /// Gets stock for a specific warehouse.
    /// </summary>
    public List<Stock> GetStockByWarehouse(int warehouseId)
    {
        const string sql = @"
            SELECT 
                s.StockID,
                s.ProductID,
                s.WarehouseID,
                s.Quantity,
                s.LastUpdateDate,
                p.ProductName,
                w.WarehouseName,
                p.Price
            FROM STOCK s
            INNER JOIN PRODUCT p ON s.ProductID = p.ProductID
            INNER JOIN WAREHOUSE w ON s.WarehouseID = w.WarehouseID
            WHERE s.WarehouseID = @WarehouseID AND s.Quantity > 0
            ORDER BY p.ProductName";

        var parameters = new[]
        {
            new SqlParameter("@WarehouseID", warehouseId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);
        var result = new List<Stock>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(MapRowToStock(row));
        }

        return result;
    }

    /// <summary>
    /// Gets total stock quantity for a product.
    /// </summary>
    public int GetTotalStockForProduct(int productId)
    {
        const string sql = "SELECT ISNULL(SUM(Quantity), 0) FROM STOCK WHERE ProductID = @ProductID";

        var parameters = new[]
        {
            new SqlParameter("@ProductID", productId)
        };

        var result = _sqlHelper.ExecuteScalar(sql, parameters);
        return Convert.ToInt32(result);
    }

    private Stock MapRowToStock(DataRow row)
    {
        return new Stock
        {
            StockID = Convert.ToInt32(row["StockID"]),
            ProductID = Convert.ToInt32(row["ProductID"]),
            WarehouseID = Convert.ToInt32(row["WarehouseID"]),
            Quantity = Convert.ToInt32(row["Quantity"]),
            LastUpdateDate = Convert.ToDateTime(row["LastUpdateDate"]),
            ProductName = row["ProductName"].ToString(),
            WarehouseName = row["WarehouseName"].ToString(),
            Price = row["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Price"])
        };
    }
}
