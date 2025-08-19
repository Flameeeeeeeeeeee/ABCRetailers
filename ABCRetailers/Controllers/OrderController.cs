using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using System.Text.Json;


namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
