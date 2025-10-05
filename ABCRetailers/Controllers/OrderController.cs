//using Microsoft.AspNetCore.Mvc;
//using ABCRetailers.Models;
//using ABCRetailers.Models.ViewModels;
//using ABCRetailers.Services;
//using ABCRetailersFunctions.Models;

//namespace ABCRetailers.Controllers
//{
//    public class OrderController : Controller
//    {
//        private readonly IFunctionsApi _functionsApi;

//        public OrderController(IFunctionsApi functionsApi)
//        {
//            _functionsApi = functionsApi;
//        }

//        // GET: Orders
//        public async Task<IActionResult> Index()
//        {
//            var orders = await _functionsApi.GetOrdersAsync();
//            return View(orders);
//        }

//        // GET: Orders/Create
//        public async Task<IActionResult> Create()
//        {
//            var customers = await _functionsApi.GetCustomersAsync();
//            var products = await _functionsApi.GetProductsAsync();

//            var viewModel = new OrderCreateViewModel
//            {
//                Customers = customers,
//                Products = products
//            };

//            return View(viewModel);
//        }

//        // POST: Orders/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(OrderCreateViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                await PopulateDropdowns(model);
//                return View(model);
//            }

//            try
//            {
//                var orderDto = new OrderDto
//                {
//                    CustomerId = model.CustomerId,
//                    ProductId = model.ProductId,
//                    OrderDate = model.OrderDate,
//                    Quantity = model.Quantity
//                };

//                await _functionsApi.CreateOrderAsync(orderDto);

//                TempData["Success"] = "Order created successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
//                await PopulateDropdowns(model);
//                return View(model);
//            }
//        }

//        // GET: Orders/Details/{id}
//        public async Task<IActionResult> Details(string id)
//        {
//            if (string.IsNullOrEmpty(id)) return NotFound();

//            var order = await _functionsApi.GetOrderAsync(id);
//            if (order == null) return NotFound();

//            return View(order);
//        }

//        // GET: Orders/Edit/{id}
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (string.IsNullOrEmpty(id)) return NotFound();

//            var order = await _functionsApi.GetOrderAsync(id);
//            if (order == null) return NotFound();

//            return View(order);
//        }

//        // POST: Orders/Edit/{id}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(string id, Order order)
//        {
//            if (id != order.OrderId) return BadRequest();
//            if (!ModelState.IsValid) return View(order);

//            try
//            {
//                // Only update Status and Quantity
//                var updateDto = new OrderDto
//                {
//                    Quantity = order.Quantity,
//                    Status = order.Status
//                };

//                await _functionsApi.UpdateOrderStatusAsync(id, updateDto.Status);

//                TempData["Success"] = "Order updated successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
//                return View(order);
//            }
//        }

//        // POST: Orders/Delete/{id}
//        [HttpPost]
//        public async Task<IActionResult> Delete(string id)
//        {
//            try
//            {
//                await _functionsApi.DeleteOrderAsync(id);
//                TempData["Success"] = "Order deleted successfully!";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Error deleting order: {ex.Message}";
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        // AJAX: Get product price & stock
//        [HttpGet]
//        public async Task<JsonResult> GetProductPrice(string productId)
//        {
//            try
//            {
//                var product = await _functionsApi.GetProductAsync(productId);
//                if (product != null)
//                {
//                    return Json(new
//                    {
//                        success = true,
//                        price = product.Price,
//                        stock = product.StockAvailable,
//                        productName = product.ProductName
//                    });
//                }
//                return Json(new { success = false });
//            }
//            catch
//            {
//                return Json(new { success = false });
//            }
//        }

//        // Helper: Populate dropdowns
//        private async Task PopulateDropdowns(OrderCreateViewModel model)
//        {
//            model.Customers = await _functionsApi.GetCustomersAsync();
//            model.Products = await _functionsApi.GetProductsAsync();
//        }
//    }
//}
