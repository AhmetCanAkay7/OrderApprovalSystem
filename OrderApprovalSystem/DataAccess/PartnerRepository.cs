using System.Data;
using Microsoft.Data.SqlClient;
using OrderApprovalSystem.Models;

namespace OrderApprovalSystem.DataAccess;

public class PartnerRepository
{
    private readonly SqlHelper _sqlHelper;

    public PartnerRepository(SqlHelper sqlHelper)
    {
        _sqlHelper = sqlHelper;
    }

    /// <summary>
    /// Gets all partners.
    /// </summary>
    public List<Partner> GetAllPartners()
    {
        const string sql = @"
            SELECT 
                PartnerID,
                PartnerName,
                Phone
            FROM PARTNER
            ORDER BY PartnerName";

        var dataTable = _sqlHelper.GetDataTable(sql);
        var result = new List<Partner>();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(MapRowToPartner(row));
        }

        return result;
    }

    /// <summary>
    /// Gets partner by ID.
    /// </summary>
    public Partner? GetPartnerById(int partnerId)
    {
        const string sql = @"
            SELECT 
                PartnerID,
                PartnerName,
                Phone
            FROM PARTNER
            WHERE PartnerID = @PartnerID";

        var parameters = new[]
        {
            new SqlParameter("@PartnerID", partnerId)
        };

        var dataTable = _sqlHelper.GetDataTable(sql, parameters);

        if (dataTable.Rows.Count == 0)
            return null;

        return MapRowToPartner(dataTable.Rows[0]);
    }

    private Partner MapRowToPartner(DataRow row)
    {
        return new Partner
        {
            PartnerID = Convert.ToInt32(row["PartnerID"]),
            PartnerName = row["PartnerName"].ToString() ?? string.Empty,
            Phone = row["Phone"] == DBNull.Value ? null : row["Phone"].ToString()
        };
    }
}
