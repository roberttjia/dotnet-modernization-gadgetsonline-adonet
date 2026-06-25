using System.Collections.Generic;
using GadgetsOnline.Data.Reports;

namespace GadgetsOnline.Reporting.Models
{
    /// <summary>Bundles the reports shown on the dashboard.</summary>
    public class DashboardViewModel
    {
        public List<CategorySalesRow> SalesByCategory { get; set; } = new();
        public List<TopProductRow> TopProducts { get; set; } = new();
        public List<MonthlyRevenueRow> RevenueByMonth { get; set; } = new();
    }
}
