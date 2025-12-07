using Microsoft.AspNetCore.Mvc;
using OrderApprovalSystem.DataAccess;

namespace OrderApprovalSystem.Controllers;

public class ProductController : Controller
{
    private readonly ProductRepository _productRepository;
    private readonly StockRepository _stockRepository;

    public ProductController(
        ProductRepository productRepository,
        StockRepository stockRepository)
    {
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    /// <summary>
    /// Product listing.
    /// </summary>
    public IActionResult Index()
    {
        var products = _productRepository.GetAllProducts();
        ViewBag.Categories = _productRepository.GetAllCategories();
        return View(products);
    }

    /// <summary>
    /// Product details with stock information.
    /// </summary>
    public IActionResult Details(int id)
    {
        var product = _productRepository.GetProductById(id);
        
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.Stock = _stockRepository.GetStockByProduct(id);
        ViewBag.TotalStock = _stockRepository.GetTotalStockForProduct(id);

        return View(product);
    }

    /// <summary>
    /// Inventory valuation report.
    /// </summary>
    public IActionResult Inventory()
    {
        var inventory = _stockRepository.GetInventoryValuation();
        ViewBag.Warehouses = _stockRepository.GetAllWarehouses();
        return View(inventory);
    }
}
