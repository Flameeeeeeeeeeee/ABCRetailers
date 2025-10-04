using Microsoft.AspNetCore.Mvc;
using ABCRetailersFunctions.Models; // Use Functions DTO
using ABCRetailers.Services;



namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public CustomerController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        // ---------------- Index ----------------
        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _functionsApi.GetCustomersAsync(); // returns List<CustomerDto>
            return View(customers);
        }

        // ---------------- Create ----------------
        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerDto customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                await _functionsApi.CreateCustomerAsync(customer);
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                return View(customer);
            }
        }

        // ---------------- Edit ----------------
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _functionsApi.GetCustomerAsync(id); // returns CustomerDto
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerDto customer)
        {
            if (id != customer.CustomerId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(customer);

            try
            {
                await _functionsApi.UpdateCustomerAsync(id, customer);
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(customer);
            }
        }

        // ---------------- Delete ----------------
        // POST: Customers/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _functionsApi.DeleteCustomerAsync(id);
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
