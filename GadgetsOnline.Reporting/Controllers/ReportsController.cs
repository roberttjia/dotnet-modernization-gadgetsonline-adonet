using GadgetsOnline.Data.Reports;
using GadgetsOnline.Reporting.Models;
using Microsoft.AspNetCore.Mvc;

namespace GadgetsOnline.Reporting.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportRepository _reports;

        public ReportsController(ReportRepository reports)
        {
            _reports = reports;
        }

        // Dashboard: all three reports on one page.
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                SalesByCategory = _reports.GetSalesByCategory(),
                TopProducts = _reports.GetTopSellingProducts(10),
                RevenueByMonth = _reports.GetRevenueByMonth()
            };
            return View(model);
        }

        public IActionResult SalesByCategory()
        {
            return View(_reports.GetSalesByCategory());
        }

        public IActionResult TopProducts()
        {
            return View(_reports.GetTopSellingProducts(10));
        }

        public IActionResult RevenueByMonth()
        {
            return View(_reports.GetRevenueByMonth());
        }
    }
}
