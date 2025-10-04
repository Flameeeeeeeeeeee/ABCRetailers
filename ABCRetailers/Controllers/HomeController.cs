using System.Diagnostics;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public HomeController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch data using the Functions API
            var products = await _functionsApi.GetProductsAsync();
            var customers = await _functionsApi.GetCustomersAsync();
            var orders = await _functionsApi.GetOrdersAsync();

            // Prepare view model
            var viewModel = new HomeViewModel
            {
                FeaturedProducts = products.Take(5)
                                           .Select(dto => new Product
                                           {
                                               ProductName = dto.ProductName,
                                               Description = dto.Description,
                                               Price = dto.Price,
                                               StockAvailable = dto.StockAvailable,
                                               ImageUrl = dto.ImageUrl ?? string.Empty
                                           })
                                           .ToList(),
                ProductCount = products.Count(),
                CustomerCount = customers.Count,
                OrderCount = orders.Count()
            };

            return View(viewModel);
        }

        public IActionResult Privacy() => View();

        public IActionResult ContactUs() => View();

        [HttpPost]
        public async Task<IActionResult> InitializeStorage()
        {
            try
            {
                // Trigger some API calls to ensure connection works
                await _functionsApi.GetCustomersAsync();
                await _functionsApi.GetProductsAsync();
                await _functionsApi.GetOrdersAsync();

                TempData["Success"] = "API storage initialized successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to initialize API storage: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
