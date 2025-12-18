using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.Controllers;

public class EmployeePanelController : Controller
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly OrderRepository _orderRepository;

    public EmployeePanelController(
        EmployeeRepository employeeRepository,
        OrderRepository orderRepository)
    {
        _employeeRepository = employeeRepository;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Dashboard for employees with approval roles.
    /// Shows pending approvals based on their role.
    /// </summary>
    public IActionResult Dashboard()
    {
        // Check if user is logged in as Employee
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Employee")
        {
            TempData["Error"] = "Please login as an employee to access this page.";
            return RedirectToAction("Login", "Account");
        }

        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        // Check if employee has approval role
        var hasRole = HttpContext.Session.GetString("HasApprovalRole") == "true";
        if (!hasRole)
        {
            return RedirectToAction("Welcome");
        }

        var employee = _employeeRepository.GetEmployeeById(employeeId.Value);
        if (employee == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // Get pending approvals for this employee
        var pendingApprovals = _orderRepository.GetPendingApprovals(employeeId.Value);

        ViewBag.Employee = employee;
        ViewBag.RoleName = HttpContext.Session.GetString("RoleName");

        return View(pendingApprovals);
    }

    /// <summary>
    /// Welcome page for employees without approval roles.
    /// </summary>
    public IActionResult Welcome()
    {
        // Check if user is logged in as Employee
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Employee")
        {
            TempData["Error"] = "Please login as an employee to access this page.";
            return RedirectToAction("Login", "Account");
        }

        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var employee = _employeeRepository.GetEmployeeById(employeeId.Value);
        if (employee == null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        var viewModel = new EmployeeWelcomeViewModel
        {
            Employee = employee,
            WelcomeMessage = GetWelcomeMessage(),
            CurrentTime = DateTime.Now
        };

        return View(viewModel);
    }

    /// <summary>
    /// View order details (for approved employees to review before approving).
    /// </summary>
    public IActionResult OrderDetails(int id)
    {
        var userType = HttpContext.Session.GetString("UserType");
        if (userType != "Employee")
        {
            return RedirectToAction("Login", "Account");
        }

        var employeeId = HttpContext.Session.GetInt32("EmployeeID");
        if (!employeeId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var viewModel = _orderRepository.GetOrderDetails(id);
        if (viewModel.Order.OrderID == 0)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("Dashboard");
        }

        // Check if current user can approve
        var (canApprove, empId, roleName) = _orderRepository.CanEmployeeApprove(id, employeeId.Value);
        viewModel.CanUserApprove = canApprove;
        viewModel.CurrentUserEmployeeID = empId;
        viewModel.CurrentUserRole = roleName;

        return View(viewModel);
    }

    /// <summary>
    /// Process approval action.
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

        // Verify the employee can approve this order
        var (canApprove, _, _) = _orderRepository.CanEmployeeApprove(model.OrderID, employeeId.Value);
        if (!canApprove)
        {
            TempData["Error"] = "You are not authorized to approve this order at the current step.";
            return RedirectToAction("Dashboard");
        }

        if (model.IsApproved)
        {
            _orderRepository.UpdateOrderStep(model.OrderID);
            TempData["Success"] = "Order has been approved successfully!";
        }
        else
        {
            TempData["Warning"] = "Order approval was declined.";
        }

        return RedirectToAction("Dashboard");
    }

    private string GetWelcomeMessage()
    {
        var hour = DateTime.Now.Hour;
        if (hour < 12)
            return "Good Morning";
        else if (hour < 17)
            return "Good Afternoon";
        else
            return "Good Evening";
    }
}
