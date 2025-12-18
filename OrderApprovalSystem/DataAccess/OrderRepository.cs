using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.Models;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.DataAccess;

public class OrderRepository
{
    private readonly SqlHelper _sqlHelper;

    public OrderRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets all orders from the dashboard view.
    /// </summary>
    public List<DashboardViewModel> GetDashboard()
    {
        const string sql = @"
            SELECT 
                o.OrderID,
                p.PartnerName,
                o.CreationTime,
                o.TotalPrice,
                o.Status,
                o.CurrentStepValue,
                CASE o.Status 
                    WHEN 0 THEN 'Completed'
                    WHEN 1 THEN 'Pending Approval'
                    ELSE 'Unknown'
                END AS StatusText,
                CASE o.CurrentStepValue
                    WHEN 0 THEN 'Commercial Approval'
                    WHEN 1 THEN 'Technical Approval'
                    WHEN 2 THEN 'Paraf Approval'
                    WHEN 3 THEN 'All Approvals Complete'
                    ELSE 'Unknown'
                END AS CurrentStepText,
                o.OrdererName + ' ' + o.OrdererSurname AS OrdererFullName
            FROM [ORDER] o
            INNER JOIN PARTNER p ON o.PartnerID = p.PartnerID
            ORDER BY o.CreationTime DESC";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<DashboardViewModel>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new DashboardViewModel
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                PartnerName = row["PartnerName"].ToString() ?? string.Empty,
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                TotalPrice = row["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPrice"]),
                Status = Convert.ToInt32(row["Status"]),
                CurrentStepValue = Convert.ToInt32(row["CurrentStepValue"]),
                StatusText = row["StatusText"].ToString() ?? string.Empty,
                CurrentStepText = row["CurrentStepText"].ToString() ?? string.Empty,
                OrdererFullName = row["OrdererFullName"].ToString() ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// Gets pending approvals for a specific employee based on their role.
    /// Only returns orders where the employee's role matches the current approval step.
    /// RoleID mapping: 1=Commercial(step 0), 2=Technical(step 1), 3=Paraf(step 2)
    /// </summary>
    public List<PendingApprovalViewModel> GetPendingApprovals(int employeeId)
    {
        const string sql = @"
            SELECT 
                o.OrderID,
                p.PartnerName,
                o.CreationTime,
                o.TotalPrice,
                ar.RoleName,
                oa.EmployeeID,
                o.OrdererName + ' ' + o.OrdererSurname AS OrdererFullName
            FROM ORDER_APPROVER oa
            INNER JOIN [ORDER] o ON oa.OrderID = o.OrderID
            INNER JOIN PARTNER p ON o.PartnerID = p.PartnerID
            INNER JOIN EMPLOYEE e ON oa.EmployeeID = e.EmployeeID
            LEFT JOIN APPROVAL_ROLE ar ON e.RoleID = ar.RoleID
            WHERE oa.EmployeeID = @EmployeeID 
              AND o.Status = 1
              AND (
                  (e.RoleID = 1 AND o.CurrentStepValue = 0) OR  -- Commercial approves step 0
                  (e.RoleID = 2 AND o.CurrentStepValue = 1) OR  -- Technical approves step 1
                  (e.RoleID = 3 AND o.CurrentStepValue = 2)     -- Paraf approves step 2
              )
            ORDER BY o.CreationTime DESC";

        var parameters = new[]
        {
            new SqlParameter("@EmployeeID", employeeId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);
        var result = new List<PendingApprovalViewModel>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new PendingApprovalViewModel
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                PartnerName = row["PartnerName"].ToString() ?? string.Empty,
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                TotalPrice = row["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPrice"]),
                RoleName = row["RoleName"]?.ToString() ?? string.Empty,
                EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                OrdererFullName = row["OrdererFullName"].ToString() ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// Gets detailed information about an order.
    /// </summary>
    public OrderDetailsViewModel GetOrderDetails(int orderId)
    {
        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId)
        };

        // Get order header
        const string orderSql = @"
            SELECT 
                o.*,
                p.PartnerName
            FROM [ORDER] o
            INNER JOIN PARTNER p ON o.PartnerID = p.PartnerID
            WHERE o.OrderID = @OrderID";

        var orderTable = _sqlHelper.GetDataTable(orderSql, parameters);

        var viewModel = new OrderDetailsViewModel();

        if (orderTable.Rows.Count > 0)
        {
            var row = orderTable.Rows[0];
            viewModel.Order = new Order
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                PartnerID = Convert.ToInt32(row["PartnerID"]),
                CurrentStepValue = Convert.ToInt32(row["CurrentStepValue"]),
                OrdererName = row["OrdererName"] == DBNull.Value ? null : row["OrdererName"].ToString(),
                OrdererSurname = row["OrdererSurname"] == DBNull.Value ? null : row["OrdererSurname"].ToString(),
                Status = Convert.ToInt32(row["Status"]),
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                TotalPrice = row["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPrice"]),
                PaymentTerm = row["PaymentTerm"] == DBNull.Value ? null : row["PaymentTerm"].ToString(),
                Currency = row["Currency"] == DBNull.Value ? null : row["Currency"].ToString(),
                OrderNote = row["OrderNote"] == DBNull.Value ? null : row["OrderNote"].ToString(),
                PartnerName = row["PartnerName"].ToString()
            };
        }

        // Get order items
        const string itemsSql = @"
            SELECT 
                oi.*,
                pr.ProductName,
                pr.ProductCode
            FROM ORDER_ITEM oi
            INNER JOIN PRODUCT pr ON oi.ProductID = pr.ProductID
            WHERE oi.OrderID = @OrderID";

        var itemsParams = new[] { new SqlParameter("@OrderID", orderId) };
        var itemsTable = _sqlHelper.GetDataTable(itemsSql, itemsParams);

        foreach (DataRow row in itemsTable.Rows)
        {
            viewModel.OrderItems.Add(new OrderItem
            {
                OrderItemID = Convert.ToInt32(row["OrderItemID"]),
                OrderID = Convert.ToInt32(row["OrderID"]),
                ProductID = Convert.ToInt32(row["ProductID"]),
                OrderItemName = row["OrderItemName"] == DBNull.Value ? null : row["OrderItemName"].ToString(),
                Unit = row["Unit"] == DBNull.Value ? null : row["Unit"].ToString(),
                OrderItemPrice = row["OrderItemPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OrderItemPrice"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Note = row["Note"] == DBNull.Value ? null : row["Note"].ToString(),
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                Currency = row["Currency"] == DBNull.Value ? null : row["Currency"].ToString(),
                ProductName = row["ProductName"].ToString(),
                ProductCode = row["ProductCode"] == DBNull.Value ? null : row["ProductCode"].ToString()
            });
        }

        // Get approvers
        const string approversSql = @"
            SELECT 
                oa.OrderID,
                oa.EmployeeID,
                e.Name + ' ' + e.Surname AS EmployeeName,
                ar.RoleName
            FROM ORDER_APPROVER oa
            INNER JOIN EMPLOYEE e ON oa.EmployeeID = e.EmployeeID
            LEFT JOIN APPROVAL_ROLE ar ON e.RoleID = ar.RoleID
            WHERE oa.OrderID = @OrderID";

        var approversParams = new[] { new SqlParameter("@OrderID", orderId) };
        var approversTable = _sqlHelper.GetDataTable(approversSql, approversParams);

        foreach (DataRow row in approversTable.Rows)
        {
            viewModel.Approvers.Add(new OrderApprover
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                EmployeeName = row["EmployeeName"].ToString(),
                RoleName = row["RoleName"]?.ToString()
            });
        }

        return viewModel;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    public int CreateOrder(int partnerId, string ordererName, string ordererSurname, string? orderNote, string? paymentTerm, string? currency)
    {
        const string sql = @"
            INSERT INTO [ORDER] (PartnerID, CurrentStepValue, OrdererName, OrdererSurname, Status, CreationTime, TotalPrice, PaymentTerm, Currency, OrderNote)
            OUTPUT INSERTED.OrderID
            VALUES (@PartnerID, 0, @OrdererName, @OrdererSurname, 1, GETDATE(), 0, @PaymentTerm, @Currency, @OrderNote)";

        var parameters = new[]
        {
            new SqlParameter("@PartnerID", partnerId),
            new SqlParameter("@OrdererName", ordererName),
            new SqlParameter("@OrdererSurname", ordererSurname),
            new SqlParameter("@PaymentTerm", (object?)paymentTerm ?? DBNull.Value),
            new SqlParameter("@Currency", (object?)currency ?? DBNull.Value),
            new SqlParameter("@OrderNote", (object?)orderNote ?? DBNull.Value)
        };

        var result = _sqlHelper.ExecuteScalar(sql, parameters);
        var orderId = Convert.ToInt32(result);

        return orderId;
    }

    /// <summary>
    /// Assigns an approver to an order.
    /// </summary>
    public void AssignApprover(int orderId, int employeeId)
    {
        const string sql = @"
            INSERT INTO ORDER_APPROVER (OrderID, EmployeeID)
            VALUES (@OrderID, @EmployeeID)";

        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId),
            new SqlParameter("@EmployeeID", employeeId)
        };

        _sqlHelper.ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// Adds an item to an order.
    /// </summary>
    public void AddOrderItem(int orderId, int productId, int quantity, string? note, string? currency)
    {
        const string sql = @"
            DECLARE @Price DECIMAL(18,2);
            DECLARE @ProductName NVARCHAR(100);
            DECLARE @Unit NVARCHAR(20);
            
            SELECT @Price = Price, @ProductName = ProductName, @Unit = Unit 
            FROM PRODUCT WHERE ProductID = @ProductID;

            INSERT INTO ORDER_ITEM (OrderID, ProductID, OrderItemName, Unit, OrderItemPrice, Quantity, Note, CreationTime, Currency)
            VALUES (@OrderID, @ProductID, @ProductName, @Unit, @Price, @Quantity, @Note, GETDATE(), @Currency);

            -- Update order total
            UPDATE [ORDER]
            SET TotalPrice = (SELECT SUM(OrderItemPrice * Quantity) FROM ORDER_ITEM WHERE OrderID = @OrderID)
            WHERE OrderID = @OrderID";

        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId),
            new SqlParameter("@ProductID", productId),
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@Note", (object?)note ?? DBNull.Value),
            new SqlParameter("@Currency", (object?)currency ?? DBNull.Value)
        };

        _sqlHelper.ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// Updates the order step to the next level.
    /// </summary>
    public void UpdateOrderStep(int orderId)
    {
        const string sql = @"
            UPDATE [ORDER]
            SET CurrentStepValue = CurrentStepValue + 1
            WHERE OrderID = @OrderID AND Status = 1;

            -- If all steps complete (CurrentStepValue becomes 3), complete the order
            IF EXISTS (SELECT 1 FROM [ORDER] WHERE OrderID = @OrderID AND CurrentStepValue >= 3 AND Status = 1)
            BEGIN
                UPDATE [ORDER]
                SET Status = 0
                WHERE OrderID = @OrderID;
            END";

        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId)
        };

        _sqlHelper.ExecuteNonQuery(sql, parameters);
    }

    /// <summary>
    /// Gets order by ID.
    /// </summary>
    public Order? GetOrderById(int orderId)
    {
        const string sql = @"
            SELECT 
                o.*,
                p.PartnerName
            FROM [ORDER] o
            INNER JOIN PARTNER p ON o.PartnerID = p.PartnerID
            WHERE o.OrderID = @OrderID";

        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return null;

        var row = dataTable.Rows[0];
        return new Order
        {
            OrderID = Convert.ToInt32(row["OrderID"]),
            PartnerID = Convert.ToInt32(row["PartnerID"]),
            CurrentStepValue = Convert.ToInt32(row["CurrentStepValue"]),
            OrdererName = row["OrdererName"] == DBNull.Value ? null : row["OrdererName"].ToString(),
            OrdererSurname = row["OrdererSurname"] == DBNull.Value ? null : row["OrdererSurname"].ToString(),
            Status = Convert.ToInt32(row["Status"]),
            CreationTime = Convert.ToDateTime(row["CreationTime"]),
            TotalPrice = row["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPrice"]),
            PaymentTerm = row["PaymentTerm"] == DBNull.Value ? null : row["PaymentTerm"].ToString(),
            Currency = row["Currency"] == DBNull.Value ? null : row["Currency"].ToString(),
            OrderNote = row["OrderNote"] == DBNull.Value ? null : row["OrderNote"].ToString(),
            PartnerName = row["PartnerName"].ToString()
        };
    }

    /// <summary>
    /// Checks if an employee can approve an order at the current step.
    /// Only allows approval when employee's role matches the order's current step.
    /// RoleID mapping: 1=Commercial(step 0), 2=Technical(step 1), 3=Paraf(step 2)
    /// </summary>
    public (bool CanApprove, int? EmployeeId, string? RoleName) CanEmployeeApprove(int orderId, int employeeId)
    {
        const string sql = @"
            SELECT 
                oa.EmployeeID,
                ar.RoleName
            FROM ORDER_APPROVER oa
            INNER JOIN [ORDER] o ON oa.OrderID = o.OrderID
            INNER JOIN EMPLOYEE e ON oa.EmployeeID = e.EmployeeID
            LEFT JOIN APPROVAL_ROLE ar ON e.RoleID = ar.RoleID
            WHERE oa.OrderID = @OrderID 
              AND oa.EmployeeID = @EmployeeID
              AND o.Status = 1
              AND (
                  (e.RoleID = 1 AND o.CurrentStepValue = 0) OR  -- Commercial approves step 0
                  (e.RoleID = 2 AND o.CurrentStepValue = 1) OR  -- Technical approves step 1
                  (e.RoleID = 3 AND o.CurrentStepValue = 2)     -- Paraf approves step 2
              )";

        var parameters = new[]
        {
            new SqlParameter("@OrderID", orderId),
            new SqlParameter("@EmployeeID", employeeId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return (false, null, null);

        var row = dataTable.Rows[0];
        return (true, Convert.ToInt32(row["EmployeeID"]), row["RoleName"]?.ToString());
    }

    /// <summary>
    /// Gets all orders for a specific partner.
    /// </summary>
    public List<DashboardViewModel> GetOrdersByPartner(int partnerId)
    {
        const string sql = @"
            SELECT 
                o.OrderID,
                p.PartnerName,
                o.CreationTime,
                o.TotalPrice,
                o.Status,
                o.CurrentStepValue,
                CASE o.Status 
                    WHEN 0 THEN 'Completed'
                    WHEN 1 THEN 'Pending Approval'
                    ELSE 'Unknown'
                END AS StatusText,
                CASE o.CurrentStepValue
                    WHEN 0 THEN 'Commercial Approval'
                    WHEN 1 THEN 'Technical Approval'
                    WHEN 2 THEN 'Paraf Approval'
                    WHEN 3 THEN 'All Approvals Complete'
                    ELSE 'Unknown'
                END AS CurrentStepText,
                o.OrdererName + ' ' + o.OrdererSurname AS OrdererFullName
            FROM [ORDER] o
            INNER JOIN PARTNER p ON o.PartnerID = p.PartnerID
            WHERE o.PartnerID = @PartnerID
            ORDER BY o.CreationTime DESC";

        var parameters = new[]
        {
            new SqlParameter("@PartnerID", partnerId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);
        var result = new List<DashboardViewModel>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new DashboardViewModel
            {
                OrderID = Convert.ToInt32(row["OrderID"]),
                PartnerName = row["PartnerName"].ToString() ?? string.Empty,
                CreationTime = Convert.ToDateTime(row["CreationTime"]),
                TotalPrice = row["TotalPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalPrice"]),
                Status = Convert.ToInt32(row["Status"]),
                CurrentStepValue = Convert.ToInt32(row["CurrentStepValue"]),
                StatusText = row["StatusText"].ToString() ?? string.Empty,
                CurrentStepText = row["CurrentStepText"].ToString() ?? string.Empty,
                OrdererFullName = row["OrdererFullName"].ToString() ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// Creates an order for a partner (without approvers assignment).
    /// </summary>
    public int CreateOrderForPartner(int partnerId, string partnerName, string? orderNote, string? paymentTerm, string? currency)
    {
        const string sql = @"
            INSERT INTO [ORDER] (PartnerID, CurrentStepValue, OrdererName, OrdererSurname, Status, CreationTime, TotalPrice, PaymentTerm, Currency, OrderNote)
            OUTPUT INSERTED.OrderID
            VALUES (@PartnerID, 0, @OrdererName, '', 1, GETDATE(), 0, @PaymentTerm, @Currency, @OrderNote)";

        var parameters = new[]
        {
            new SqlParameter("@PartnerID", partnerId),
            new SqlParameter("@OrdererName", partnerName),
            new SqlParameter("@PaymentTerm", (object?)paymentTerm ?? DBNull.Value),
            new SqlParameter("@Currency", (object?)currency ?? DBNull.Value),
            new SqlParameter("@OrderNote", (object?)orderNote ?? DBNull.Value)
        };

        var result = _sqlHelper.ExecuteScalar(sql, parameters);
        return Convert.ToInt32(result);
    }
}
