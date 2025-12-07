using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;

namespace OrderApprovalSystem.Controllers;

public class DashboardController : Controller
{
    private readonly OrderRepository _orderRepository;
    private readonly ReportRepository _reportRepository;

    public DashboardController(
        OrderRepository orderRepository,
        ReportRepository reportRepository)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
    }

    /// <summary>
    /// Main dashboard with summary and recent orders.
    /// </summary>
    public IActionResult Index()
    {
        var summary = _reportRepository.GetDashboardSummary();
        var recentOrders = _orderRepository.GetDashboard().Take(10).ToList();

        ViewBag.Summary = summary;
        return View(recentOrders);
    }

    /// <summary>
    /// Partner sales performance report.
    /// </summary>
    public IActionResult PartnerSales()
    {
        var salesData = _reportRepository.GetPartnerSalesPerformance();
        return View(salesData);
    }
}
