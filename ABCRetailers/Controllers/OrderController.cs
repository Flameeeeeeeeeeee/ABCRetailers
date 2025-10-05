using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;

using ABCRetailersFunctions.Models;


namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        // -------------------- Index --------------------
        public async Task<IActionResult> Index()
        {
            var orderDtos = await _functionsApi.GetOrdersAsync();

            // Map DTOs to MVC Models
            var orders = orderDtos.Select(d => new Order
            {
                RowKey = d.OrderId ?? string.Empty,
                CustomerId = d.CustomerId,
                Username = d.Username,
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                OrderDate = d.OrderDate,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TotalPrice = d.TotalPrice,
                Status = d.Status
            }).ToList();

            return View(orders);
        }

        // -------------------- Create --------------------
        public async Task<IActionResult> Create()
        {
            var viewModel = new OrderCreateViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                // 🚨 FIX HERE: Populate the lists NOW so the FirstOrDefault search works.
                await PopulateDropdowns(model);

                // Validate IDs (your original check, unchanged)
                if (string.IsNullOrWhiteSpace(model.CustomerId) || string.IsNullOrWhiteSpace(model.ProductId))
                {
                    ModelState.AddModelError("", "Please select both a customer and a product.");
                    // We already called PopulateDropdowns above, so we can return the view.
                    return View(model);
                }

                // Now, this search will work because the lists are populated.
                var customer = model.Customers.FirstOrDefault(c => c.RowKey == model.CustomerId);
                var product = model.Products.FirstOrDefault(p => p.RowKey == model.ProductId);

                // If the selected ID is somehow invalid (not found in the DB), this check is still important.
                if (customer == null || product == null)
                {
                    ModelState.AddModelError("", "Invalid customer or product selection.");
                    // Lists are already populated, just return view.
                    return View(model);
                }

                // Log selected IDs for debugging
                Console.WriteLine($"CustomerId: {model.CustomerId}, ProductId: {model.ProductId}");

                // Build DTO
                var orderDto = new OrderDto
                {
                    CustomerId = customer.RowKey,
                    Username = customer.Username,
                    ProductId = product.RowKey,
                    ProductName = product.ProductName,
                    Quantity = model.Quantity,
                    UnitPrice = (double)product.Price,
                    TotalPrice = (double)(product.Price * model.Quantity),
                    OrderDate = DateTime.SpecifyKind(model.OrderDate, DateTimeKind.Utc),
                    Status = string.IsNullOrWhiteSpace(model.Status) ? "Submitted" : model.Status
                };

                // Serialize JSON for logging
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var json = System.Text.Json.JsonSerializer.Serialize(orderDto, options);
                Console.WriteLine("===== JSON SENT TO AZURE FUNCTION =====");
                Console.WriteLine(json);
                Console.WriteLine("=====================================");

                // Send DTO to Azure Function
                await _functionsApi.CreateOrderAsync(orderDto);

                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }




        // -------------------- Edit --------------------
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var dto = await _functionsApi.GetOrderAsync(id);
            if (dto == null) return NotFound();

            // Map DTO to ViewModel for editing
            var viewModel = new OrderCreateViewModel
            {
                CustomerId = dto.CustomerId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                OrderDate = dto.OrderDate,
                Status = dto.Status
            };
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                // Build DTO to update order
                var orderDto = new OrderDto
                {
                    OrderId = id,
                    Status = model.Status,
                    Quantity = model.Quantity
                };

                await _functionsApi.UpdateOrderStatusAsync(id, model.Status);

                TempData["Success"] = "Order updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        // -------------------- Delete --------------------
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _functionsApi.DeleteOrderAsync(id);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------- AJAX: Product Price --------------------
        [HttpGet]
        public async Task<JsonResult> GetProductPrice(string productId)
        {
            try
            {
                var productDto = await _functionsApi.GetProductAsync(productId);
                if (productDto != null)
                {
                    return Json(new
                    {
                        success = true,
                        price = productDto.Price,
                        stock = productDto.StockAvailable,
                        productName = productDto.ProductName
                    });
                }
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // -------------------- AJAX: Update Order Status --------------------
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string id, string newStatus)
        {
            try
            {
                await _functionsApi.UpdateOrderStatusAsync(id, newStatus);
                return Json(new { success = true, message = $"Order status updated to {newStatus}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // -------------------- Helpers --------------------
        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            var customerDtos = await _functionsApi.GetCustomersAsync();
            var productDtos = await _functionsApi.GetProductsAsync();

            model.Customers = customerDtos.Select(d => new Customer
            {
                CustomerId = d.CustomerId,
                Name = d.Name,
                Surname = d.Surname,
                Username = d.Username
            }).ToList();

            model.Products = productDtos.Select(d => new Product
            {
                ProductId = d.ProductId ?? string.Empty,
                ProductName = d.ProductName,
                Price = d.Price,
                StockAvailable = d.StockAvailable,
                ImageUrl = d.ImageUrl
            }).ToList();
        }
    }
}
