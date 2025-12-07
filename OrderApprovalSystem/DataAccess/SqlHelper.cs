using System.Data;
using Microsoft.Data.SqlClient;

namespace OrderApprovalSystem.DataAccess;

/// <summary>
/// Singleton SqlHelper class for centralized database connection and command management.
/// Provides methods for executing queries, stored procedures, and non-query commands.
/// </summary>
public class SqlHelper
{
    private readonly string _connectionString;
    private static SqlHelper? _instance;
    private static readonly object _lock = new();

    private SqlHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static SqlHelper GetInstance(string connectionString)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                _instance ??= new SqlHelper(connectionString);
            }
        }
        return _instance;
    }

    /// <summary>
    /// Executes a SELECT query and returns the results as a DataTable.
    /// </summary>
    public DataTable GetDataTable(string sql, SqlParameter[]? parameters = null)
    {
        var dataTable = new DataTable();
        
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(dataTable);
        
        return dataTable;
    }

    /// <summary>
    /// Executes a stored procedure and returns the results as a DataTable.
    /// </summary>
    public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[]? parameters = null)
    {
        var dataTable = new DataTable();
        
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(dataTable);
        
        return dataTable;
    }

    /// <summary>
    /// Executes an INSERT, UPDATE, or DELETE command and returns the number of affected rows.
    /// </summary>
    public int ExecuteNonQuery(string sql, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        
        connection.Open();
        return command.ExecuteNonQuery();
    }


    
    /// Executes a query and returns the first column of the first row.
    public object? ExecuteScalar(string sql, SqlParameter[]? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        
        connection.Open();
        return command.ExecuteScalar();
    }

    

    
}
