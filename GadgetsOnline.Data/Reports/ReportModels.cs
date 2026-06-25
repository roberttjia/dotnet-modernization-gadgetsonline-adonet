using System;

namespace GadgetsOnline.Data.Reports
{
    /// <summary>One row of the "sales by category" report.</summary>
    public class CategorySalesRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int OrderCount { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>One row of the "top selling products" report.</summary>
    public class TopProductRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public long SalesRank { get; set; }
    }

    /// <summary>One row of the "revenue by month" report.</summary>
    public class MonthlyRevenueRow
    {
        public DateTime MonthStart { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal RunningTotal { get; set; }
    }
}
