using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.Controllers;

public class PartnerController : Controller
{
    private readonly PartnerRepository _partnerRepository;
    private readonly ReportRepository _reportRepository;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly StockRepository _stockRepository;

    public PartnerController(
        PartnerRepository partnerRepository,
        ReportRepository reportRepository,
        OrderRepository orderRepository,
        ProductRepository productRepository,
        StockRepository stockRepository)
    {
        _partnerRepository = partnerRepository;
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    /// <summary>
    /// Partner Dashboard - main panel for logged-in partners.
    /// </summary>
    public IActionResult Dashboard()
    {
        // Check if user is logged in as Partner
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Partner")
        {
            TempData["Error"] = "Please login as a partner to access this page.";
            return RedirectToAction("Login", "Account");
        }

        var partnerId = HttpContext.Session.GetInt32("PartnerID");
        if (!partnerId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var partner = _partnerRepository.GetPartnerById(partnerId.Value);
        if (partner == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        var orders = _orderRepository.GetOrdersByPartner(partnerId.Value);

        var viewModel = new PartnerDashboardViewModel
        {
            Partner = partner,
            RecentOrders = orders.Take(5).ToList(),
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == 1),
            CompletedOrders = orders.Count(o => o.Status == 0),
            TotalSpent = orders.Sum(o => o.TotalPrice)
        };

        return View(viewModel);
    }

    /// <summary>
    /// List all orders for the logged-in partner.
    /// </summary>
    public IActionResult MyOrders()
    {
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Partner")
        {
            return RedirectToAction("Login", "Account");
        }

        var partnerId = HttpContext.Session.GetInt32("PartnerID");
        if (!partnerId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = _orderRepository.GetOrdersByPartner(partnerId.Value);
        ViewBag.PartnerName = HttpContext.Session.GetString("PartnerName");

        return View(orders);
    }

    /// <summary>
    /// Create order form for partner.
    /// </summary>
    public IActionResult CreateOrder()
    {
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Partner")
        {
            return RedirectToAction("Login", "Account");
        }

        var partnerId = HttpContext.Session.GetInt32("PartnerID");
        if (!partnerId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var viewModel = new PartnerCreateOrderViewModel
        {
            PartnerID = partnerId.Value,
            PartnerName = HttpContext.Session.GetString("PartnerName"),
            AvailableProducts = _productRepository.GetAllProducts()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Process order creation with stock check.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateOrder(PartnerCreateOrderViewModel model, int[] productIds, int[] quantities)
    {
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Partner")
        {
            return RedirectToAction("Login", "Account");
        }

        var partnerId = HttpContext.Session.GetInt32("PartnerID");
        var partnerName = HttpContext.Session.GetString("PartnerName");

        if (!partnerId.HasValue || string.IsNullOrEmpty(partnerName))
        {
            return RedirectToAction("Login", "Account");
        }

        // Validate input
        if (productIds == null || quantities == null || productIds.Length == 0)
        {
            TempData["Error"] = "Please add at least one product to your order.";
            model.AvailableProducts = _productRepository.GetAllProducts();
            return View(model);
        }

        // Build items list for stock check
        var itemsToCheck = new List<(int ProductId, int Quantity)>();
        for (int i = 0; i < productIds.Length; i++)
        {
            if (productIds[i] > 0 && quantities[i] > 0)
            {
                itemsToCheck.Add((productIds[i], quantities[i]));
            }
        }

        if (itemsToCheck.Count == 0)
        {
            TempData["Error"] = "Please add at least one product with a valid quantity.";
            model.AvailableProducts = _productRepository.GetAllProducts();
            return View(model);
        }

        // Check stock availability
        var insufficientStock = _stockRepository.CheckMultipleStockAvailability(itemsToCheck);
        if (insufficientStock.Count > 0)
        {
            var stockErrors = insufficientStock.Select(s =>
                $"'{s.ProductName}': Requested {s.RequestedQuantity}, Available {s.AvailableQuantity}");

            TempData["StockError"] = "Insufficient stock for the following products:";
            TempData["StockErrorDetails"] = string.Join("|", stockErrors);

            model.AvailableProducts = _productRepository.GetAllProducts();
            return View(model);
        }

        // Create the order
        var orderId = _orderRepository.CreateOrderForPartner(
            partnerId.Value,
            partnerName,
            model.Notes,
            model.PaymentTerm,
            model.Currency);

        // Add order items
        foreach (var item in itemsToCheck)
        {
            _orderRepository.AddOrderItem(orderId, item.ProductId, item.Quantity, null, model.Currency);
        }

        TempData["Success"] = $"Order #{orderId} created successfully! It is now pending approval.";
        return RedirectToAction("MyOrders");
    }

    /// <summary>
    /// AJAX endpoint for real-time stock check.
    /// </summary>
    [HttpPost]
    public IActionResult CheckStock([FromBody] StockCheckRequest request)
    {
        if (request?.ProductId == null || request.Quantity <= 0)
        {
            return Json(new { success = false, message = "Invalid request" });
        }

        var (isAvailable, availableQty, productName) = _stockRepository.CheckStockAvailability(
            request.ProductId.Value, request.Quantity);

        return Json(new
        {
            success = isAvailable,
            productName,
            availableQuantity = availableQty,
            requestedQuantity = request.Quantity,
            message = isAvailable
                ? "Stock available"
                : $"Insufficient stock. Available: {availableQty}"
        });
    }

    // ============ ADMIN VIEWS (Original functionality) ============

    /// <summary>
    /// Partner listing (admin view).
    /// </summary>
    public IActionResult Index()
    {
        var partners = _partnerRepository.GetAllPartners();
        return View(partners);
    }

    /// <summary>
    /// Partner details (admin view).
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
    /// Partner sales performance (admin view).
    /// </summary>
    public IActionResult SalesPerformance()
    {
        var salesData = _reportRepository.GetPartnerSalesPerformance();
        return View(salesData);
    }
}

public class StockCheckRequest
{
    public int? ProductId { get; set; }
    public int Quantity { get; set; }
}
