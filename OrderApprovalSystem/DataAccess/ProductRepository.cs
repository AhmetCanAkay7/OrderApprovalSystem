using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.DataAccess;

public class ProductRepository
{
    private readonly SqlHelper _sqlHelper;

    public ProductRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    public List<Product> GetAllProducts()
    {
        const string sql = @"
            SELECT 
                p.ProductID,
                p.ProductName,
                p.Description,
                p.Price,
                p.Unit,
                p.ProductCode,
                p.MinDeliveryTime,
                p.Color,
                p.CategoryID,
                c.CategoryName
            FROM PRODUCT p
            INNER JOIN CATEGORY c ON p.CategoryID = c.CategoryID
            ORDER BY c.CategoryName, p.ProductName";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<Product>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(MapRowToProduct(row));
        }

        return result;
    }

    /// <summary>
    /// Gets product by ID.
    /// </summary>
    public Product? GetProductById(int productId)
    {
        const string sql = @"
            SELECT 
                p.ProductID,
                p.ProductName,
                p.Description,
                p.Price,
                p.Unit,
                p.ProductCode,
                p.MinDeliveryTime,
                p.Color,
                p.CategoryID,
                c.CategoryName
            FROM PRODUCT p
            INNER JOIN CATEGORY c ON p.CategoryID = c.CategoryID
            WHERE p.ProductID = @ProductID";

        var parameters = new[]
        {
            new SqlParameter("@ProductID", productId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return null;

        return MapRowToProduct(dataTable.Rows[0]);
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    public List<Category> GetAllCategories()
    {
        const string sql = "SELECT CategoryID, CategoryName FROM CATEGORY ORDER BY CategoryName";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<Category>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new Category
            {
                CategoryID = Convert.ToInt32(row["CategoryID"]),
                CategoryName = row["CategoryName"].ToString() ?? string.Empty
            });
        }

        return result;
    }

    private Product MapRowToProduct(DataRow row)
    {
        return new Product
        {
            ProductID = Convert.ToInt32(row["ProductID"]),
            ProductName = row["ProductName"].ToString() ?? string.Empty,
            Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
            Price = row["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Price"]),
            Unit = row["Unit"] == DBNull.Value ? null : row["Unit"].ToString(),
            ProductCode = row["ProductCode"] == DBNull.Value ? null : row["ProductCode"].ToString(),
            MinDeliveryTime = row["MinDeliveryTime"] == DBNull.Value ? 1 : Convert.ToInt32(row["MinDeliveryTime"]),
            Color = row["Color"] == DBNull.Value ? null : row["Color"].ToString(),
            CategoryID = Convert.ToInt32(row["CategoryID"]),
            CategoryName = row["CategoryName"].ToString()
        };
    }
}
