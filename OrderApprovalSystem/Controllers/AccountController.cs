using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.Controllers;

public class AccountController : Controller
{
    private readonly EmployeeRepository _employeeRepository;
    private readonly PartnerRepository _partnerRepository;

    public AccountController(EmployeeRepository employeeRepository, PartnerRepository partnerRepository)
    {
        _employeeRepository = employeeRepository;
        _partnerRepository = partnerRepository;
    }

    /// <summary>
    /// Login page - supports both Partner and Employee login.
    /// </summary>
    public IActionResult Login()
    {
        // If already logged in, redirect to appropriate dashboard
        var userType = HttpContext.Session.GetString("UserType");
        if (!string.IsNullOrEmpty(userType))
        {
            return RedirectToAppropriateDashboard(userType);
        }

        return View(new LoginViewModel());
    }

    /// <summary>
    /// Process login for Partner or Employee.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (model.LoginType == "Partner")
        {
            return ProcessPartnerLogin(model);
        }
        else
        {
            return ProcessEmployeeLogin(model);
        }
    }

    private IActionResult ProcessPartnerLogin(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.PartnerName))
        {
            TempData["Error"] = "Please enter a partner name.";
            return View(model);
        }

        var partner = _partnerRepository.GetPartnerByName(model.PartnerName.Trim());
        if (partner == null)
        {
            TempData["Error"] = $"Partner '{model.PartnerName}' not found. Please check the name and try again.";
            return View(model);
        }

        // Set session for Partner
        HttpContext.Session.SetString("UserType", "Partner");
        HttpContext.Session.SetInt32("PartnerID", partner.PartnerID);
        HttpContext.Session.SetString("PartnerName", partner.PartnerName);

        TempData["Success"] = $"Welcome, {partner.PartnerName}!";
        return RedirectToAction("Dashboard", "Partner");
    }

    private IActionResult ProcessEmployeeLogin(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.EmployeeEmail))
        {
            TempData["Error"] = "Please enter your email address.";
            return View(model);
        }

        var employee = _employeeRepository.GetEmployeeByEmail(model.EmployeeEmail.Trim());
        if (employee == null)
        {
            TempData["Error"] = $"Employee with email '{model.EmployeeEmail}' not found. Please check and try again.";
            return View(model);
        }

        // Set session for Employee
        HttpContext.Session.SetString("UserType", "Employee");
        HttpContext.Session.SetInt32("EmployeeID", employee.EmployeeID);
        HttpContext.Session.SetString("EmployeeName", employee.FullName);
        HttpContext.Session.SetString("EmployeeEmail", employee.Email);

        // Store role information
        if (employee.RoleID.HasValue)
        {
            HttpContext.Session.SetInt32("RoleID", employee.RoleID.Value);
            HttpContext.Session.SetString("HasApprovalRole", "true");
            HttpContext.Session.SetString("RoleName", employee.RoleName ?? "");
        }
        else
        {
            HttpContext.Session.SetString("HasApprovalRole", "false");
        }

        TempData["Success"] = $"Welcome, {employee.FullName}!";

        // Redirect based on role
        if (employee.RoleID.HasValue)
        {
            return RedirectToAction("Dashboard", "EmployeePanel");
        }
        else
        {
            return RedirectToAction("Welcome", "EmployeePanel");
        }
    }

    private IActionResult RedirectToAppropriateDashboard(string userType)
    {
        if (userType == "Partner")
        {
            return RedirectToAction("Dashboard", "Partner");
        }
        else
        {
            var hasRole = HttpContext.Session.GetString("HasApprovalRole") == "true";
            if (hasRole)
            {
                return RedirectToAction("Dashboard", "EmployeePanel");
            }
            else
            {
                return RedirectToAction("Welcome", "EmployeePanel");
            }
        }
    }

    /// <summary>
    /// Logout - clears session and redirects to login.
    /// </summary>
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out successfully.";
        return RedirectToAction("Login");
    }
}
