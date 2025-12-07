using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;

namespace OrderApprovalSystem.Controllers;

public class EmployeeController : Controller
{
    private readonly EmployeeRepository _employeeRepository;

    public EmployeeController(EmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Employee directory listing.
    /// </summary>
    public IActionResult Index()
    {
        var employees = _employeeRepository.GetEmployeeDirectory();
        return View(employees);
    }

    /// <summary>
    /// Employee details.
    /// </summary>
    public IActionResult Details(int id)
    {
        var employee = _employeeRepository.GetEmployeeById(id);
        
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }
}
