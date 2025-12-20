using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.DataAccess;

public class EmployeeRepository
{
    private readonly SqlHelper _sqlHelper;

    public EmployeeRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets employee by ID.
    /// </summary>
    public Employee? GetEmployeeById(int employeeId)
    {
        const string sql = @"
            SELECT 
                e.EmployeeID,
                e.Username,
                e.Email,
                e.Name,
                e.Surname,
                e.Phone,
                e.StartDate,
                e.DepartmentID,
                e.RoleID,
                e.ManagerID,
                d.DepartmentName,
                m.Name + ' ' + m.Surname AS ManagerName,
                ar.RoleName
            FROM EMPLOYEE e
            INNER JOIN DEPARTMENT d ON e.DepartmentID = d.DepartmentID
            LEFT JOIN EMPLOYEE m ON e.ManagerID = m.EmployeeID
            LEFT JOIN APPROVAL_ROLE ar ON e.RoleID = ar.RoleID
            WHERE e.EmployeeID = @EmployeeID";

        var parameters = new[]
        {
            new SqlParameter("@EmployeeID", employeeId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return null;

        return MapRowToEmployee(dataTable.Rows[0]);
    }

    /// <summary>
    /// Gets employee by email (for login).
    /// </summary>
    public Employee? GetEmployeeByEmail(string email)
    {
        const string sql = @"
            SELECT 
                e.EmployeeID,
                e.Username,
                e.Email,
                e.Name,
                e.Surname,
                e.Phone,
                e.StartDate,
                e.DepartmentID,
                e.RoleID,
                e.ManagerID,
                d.DepartmentName,
                m.Name + ' ' + m.Surname AS ManagerName,
                ar.RoleName
            FROM EMPLOYEE e
            INNER JOIN DEPARTMENT d ON e.DepartmentID = d.DepartmentID
            LEFT JOIN EMPLOYEE m ON e.ManagerID = m.EmployeeID
            LEFT JOIN APPROVAL_ROLE ar ON e.RoleID = ar.RoleID
            WHERE e.Email = @Email";

        var parameters = new[]
        {
            new SqlParameter("@Email", email)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return null;

        return MapRowToEmployee(dataTable.Rows[0]);
    }
    private Employee MapRowToEmployee(DataRow row)
    {
        return new Employee
        {
            EmployeeID = Convert.ToInt32(row["EmployeeID"]),
            Username = row["Username"].ToString() ?? string.Empty,
            Email = row["Email"].ToString() ?? string.Empty,
            Name = row["Name"].ToString() ?? string.Empty,
            Surname = row["Surname"].ToString() ?? string.Empty,
            Phone = row["Phone"] == DBNull.Value ? null : row["Phone"].ToString(),
            StartDate = Convert.ToDateTime(row["StartDate"]),
            DepartmentID = Convert.ToInt32(row["DepartmentID"]),
            RoleID = row["RoleID"] == DBNull.Value ? null : Convert.ToInt32(row["RoleID"]),
            ManagerID = row["ManagerID"] == DBNull.Value ? null : Convert.ToInt32(row["ManagerID"]),
            DepartmentName = row["DepartmentName"].ToString(),
            ManagerName = row["ManagerName"] == DBNull.Value ? null : row["ManagerName"].ToString(),
            RoleName = row["RoleName"] == DBNull.Value ? null : row["RoleName"].ToString()
        };
    }
}
