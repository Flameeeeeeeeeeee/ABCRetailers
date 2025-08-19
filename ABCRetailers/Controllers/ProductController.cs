using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
