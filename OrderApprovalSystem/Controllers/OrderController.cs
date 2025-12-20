using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.Controllers;

public class OrderController : Controller
{
    private readonly OrderRepository _orderRepository;
    private readonly PartnerRepository _partnerRepository;
    private readonly ProductRepository _productRepository;
    private readonly EmployeeRepository _employeeRepository;

    public OrderController(
        OrderRepository orderRepository,
        PartnerRepository partnerRepository,
        ProductRepository productRepository,
        EmployeeRepository employeeRepository)
    {
        _orderRepository = orderRepository;
        _partnerRepository = partnerRepository;
        _productRepository = productRepository;
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Lists all orders.
    /// </summary>
    public IActionResult Index()
    {
        var orders = _orderRepository.GetDashboard();
        return View(orders);
    }

    /// <summary>
    /// Shows order details.
    /// </summary>
    public IActionResult Details(int id)
    {
        var viewModel = _orderRepository.GetOrderDetails(id);

        if (viewModel.Order.OrderID == 0)
        {
            return NotFound();
        }

        // Check if current user can approve
        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (employeeId.HasValue)
        {
            var (canApprove, empId, roleName) = _orderRepository.CanEmployeeApprove(id, employeeId.Value);
            viewModel.CanUserApprove = canApprove;
            viewModel.CurrentUserEmployeeID = empId;
            viewModel.CurrentUserRole = roleName;
        }

        return View(viewModel);
    }

    /// <summary>
    /// Shows the create order form.
    /// </summary>
    public IActionResult Create()
    {
        var viewModel = new CreateOrderViewModel
        {
            AvailablePartners = _partnerRepository.GetAllPartners(),
            AvailableProducts = _productRepository.GetAllProducts()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Processes order creation.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateOrderViewModel model, int[] productIds, int[] quantities)
    {
        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            TempData["Error"] = "Please login to create orders.";
            return RedirectToAction("Login", "Account");
        }

        if (model.PartnerID == 0)
        {
            ModelState.AddModelError("PartnerID", "Please select a partner.");
            model.AvailablePartners = _partnerRepository.GetAllPartners();
            model.AvailableProducts = _productRepository.GetAllProducts();
            return View(model);
        }

        // Get employee info for orderer name
        var employee = _employeeRepository.GetEmployeeById(employeeId.Value);
        var ordererName = employee?.Name ?? "Unknown";
        var ordererSurname = employee?.Surname ?? "";

        // Create the order
        var orderId = _orderRepository.CreateOrder(model.PartnerID, ordererName, ordererSurname, model.Notes, null, null);

        // Assign random approvers using stored procedure
        _orderRepository.AssignRandomApprovers(orderId);

        // Add order items
        if (productIds != null && quantities != null)
        {
            for (int i = 0; i < productIds.Length; i++)
            {
                if (productIds[i] > 0 && quantities[i] > 0)
                {
                    _orderRepository.AddOrderItem(orderId, productIds[i], quantities[i], null, null);
                }
            }
        }

        TempData["Success"] = $"Order #{orderId} created successfully!";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    /// <summary>
    /// Shows pending approvals for the current user.
    /// </summary>
    public IActionResult PendingApprovals()
    {
        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            TempData["Error"] = "Please login to view pending approvals.";
            return RedirectToAction("Login", "Account");
        }

        var pendingApprovals = _orderRepository.GetPendingApprovals(employeeId.Value);
        return View(pendingApprovals);
    }

    /// <summary>
    /// Processes an approval action (advances order step).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Approve(ApprovalActionViewModel model)
    {
        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            TempData["Error"] = "Please login to approve orders.";
            return RedirectToAction("Login", "Account");
        }

        if (model.IsApproved)
        {
            _orderRepository.UpdateOrderStep(model.OrderID);
            TempData["Success"] = "Order has been approved successfully!";
        }
        else
        {
            TempData["Error"] = "Order approval declined.";
        }

        return RedirectToAction(nameof(Details), new { id = model.OrderID });
    }
}
