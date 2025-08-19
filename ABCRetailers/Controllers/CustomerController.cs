using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public HomeController(IAzureStorageService storageService)
        { 
            _storageService = storageService; 
        }

        public async Task<ActionResult> Index()
        { var products await = await _storageService.GetAllEntitiesAsync<Product>(); }
        public IActionResult Index()
        {
            return View();
        }
    }
}
