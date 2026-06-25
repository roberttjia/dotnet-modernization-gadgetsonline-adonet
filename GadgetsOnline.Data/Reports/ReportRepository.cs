using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace GadgetsOnline.Data.Reports
{
    /// <summary>
    /// Read-only reporting data access via stored procedures. Used by the
    /// reporting application; lives in the shared data library so all DB
    /// access stays in one place.
    /// </summary>
    public class ReportRepository
    {
        private readonly Database _database;

        public ReportRepository(Database database)
        {
            _database = database;
        }

        public List<CategorySalesRow> GetSalesByCategory()
        {
            var rows = new List<CategorySalesRow>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Report_SalesByCategory", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new CategorySalesRow
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    OrderCount = reader.GetInt32(reader.GetOrdinal("OrderCount")),
                    UnitsSold = reader.GetInt32(reader.GetOrdinal("UnitsSold")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"))
                });
            }

            return rows;
        }

        public List<TopProductRow> GetTopSellingProducts(int count)
        {
            var rows = new List<TopProductRow>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Report_TopSellingProducts", connection);
            command.Parameters.AddWithValue("@Count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new TopProductRow
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    UnitsSold = reader.GetInt32(reader.GetOrdinal("UnitsSold")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue")),
                    SalesRank = reader.GetInt64(reader.GetOrdinal("SalesRank"))
                });
            }

            return rows;
        }

        public List<MonthlyRevenueRow> GetRevenueByMonth()
        {
            var rows = new List<MonthlyRevenueRow>();

            using var connection = _database.CreateOpenConnection();
            using var command = Database.CreateStoredProcCommand("dbo.Report_RevenueByMonth", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new MonthlyRevenueRow
                {
                    MonthStart = reader.GetDateTime(reader.GetOrdinal("MonthStart")),
                    OrderCount = reader.GetInt32(reader.GetOrdinal("OrderCount")),
                    Revenue = reader.GetDecimal(reader.GetOrdinal("Revenue")),
                    RunningTotal = reader.GetDecimal(reader.GetOrdinal("RunningTotal"))
                });
            }

            return rows;
        }
    }
}
