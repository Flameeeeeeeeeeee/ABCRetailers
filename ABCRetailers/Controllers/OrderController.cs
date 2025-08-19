using Microsoft.AspNetCore.Mvc;

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
