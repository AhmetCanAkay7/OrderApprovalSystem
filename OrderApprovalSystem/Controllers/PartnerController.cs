using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;

namespace OrderApprovalSystem.Controllers;

public class PartnerController : Controller
{
    private readonly PartnerRepository _partnerRepository;
    private readonly ReportRepository _reportRepository;

    public PartnerController(
        PartnerRepository partnerRepository,
        ReportRepository reportRepository)
    {
        _partnerRepository = partnerRepository;
        _reportRepository = reportRepository;
    }

    /// <summary>
    /// Partner listing.
    /// </summary>
    public IActionResult Index()
    {
        var partners = _partnerRepository.GetAllPartners();
        return View(partners);
    }

    /// <summary>
    /// Partner details.
    /// </summary>
    public IActionResult Details(int id)
    {
        var partner = _partnerRepository.GetPartnerById(id);
        
        if (partner == null)
        {
            return NotFound();
        }

        return View(partner);
    }

    /// <summary>
    /// Partner sales performance.
    /// </summary>
    public IActionResult SalesPerformance()
    {
        var salesData = _reportRepository.GetPartnerSalesPerformance();
        return View(salesData);
    }
}
