using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;
using OrderApprovalSystem.ViewModels;

namespace OrderApprovalSystem.Controllers;

public class AccountController : Controller
{
    private readonly EmployeeRepository _employeeRepository;

    public AccountController(EmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Login page.
    /// </summary>
    public IActionResult Login()
    {
        // Get all employees for the dropdown
        ViewBag.Employees = _employeeRepository.GetAllEmployees();
        return View();
    }

    /// <summary>
    /// Process login.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (model.EmployeeID > 0)
        {
            var employee = _employeeRepository.GetEmployeeById(model.EmployeeID);
            if (employee != null)
            {
                HttpContext.Session.SetInt32("EmployeeID", employee.EmployeeID);
                HttpContext.Session.SetString("EmployeeName", employee.FullName);
                HttpContext.Session.SetString("EmployeeEmail", employee.Email);

                TempData["Success"] = $"Welcome, {employee.FullName}!";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        TempData["Error"] = "Invalid login. Please select an employee.";
        ViewBag.Employees = _employeeRepository.GetAllEmployees();
        return View(model);
    }

    /// <summary>
    /// Logout.
    /// </summary>
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Login");
    }
}
