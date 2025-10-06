using System.Reflection.Metadata;
using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ABCRetailers.Controllers
{

    //Handles all CRUD operations for Customer entities via the MVC application.
   
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _api;// Communicates with the Azure Functions API for all data interactions.
        public CustomerController(IFunctionsApi api) => _api = api; //injecting Api 

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var customers = await _api.GetCustomersAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                customers = customers
                    .Where(c =>
                        (c.Name != null && c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||//search bar here
                        (c.Email != null && c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    
                    .ToList();
            }

            ViewData["SearchTerm"] = searchTerm; // Pass term to the view to keep it in the input box
            return View(customers);
        }

        // -------------------- Create (GET) --------------------
        //view for creation of ccustomer
        public IActionResult Create() => View();

        // -------------------- Create (POST) --------------------
        //handles submission of post
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)//breakpoint
        {
            if (!ModelState.IsValid) return View(customer);   // Check for client-side and server-side validation errors
            try
            {
                // Call the Azure Functions API to persist the new customer
                await _api.CreateCustomerAsync(customer);
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            { // Log and display API errors to the user
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                return View(customer);
            }
        }
        // -------------------- Edit (GET) --------------------
        //displays view for editing customer
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            //grab customer via id
            var customer = await _api.GetCustomerAsync(id);
            return customer is null ? NotFound() : View(customer);
        }
        //Handles the submission of the edited customer form.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);
            try
            {
                // Call the Azure Functions API to update the customer record
                await _api.UpdateCustomerAsync(customer.Id, customer);
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(customer);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _api.DeleteCustomerAsync(id);
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
