using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;



namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public CustomerController(IAzureStorageService storageService)
        {
            _storageService = storageService;

        }

        public async Task<IActionResult> Index()
        {
            var customers = await _storageService.GetAllEntitiesAsync<Customer>();
            return View(customers);
        }
        public IActionResult Create()
        {
            return View();
        }
        //create action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _storageService.AddEntityAsync(customer);
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating customer:{ex.Message}");
                }
            }
            return View(customer);
        }

        //Edit action
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var customer = await _storageService.GetEntityAsync<Customer>("Customer", id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //update entity
                    var originalCustomer = await _storageService.GetEntityAsync<Customer>("Customer", customer.RowKey); //retrives orignal customer entity, so it can be edited
                    if (originalCustomer == null)
                    {
                        return NotFound();
                    }
                    // Update fields (model defined data)
                    originalCustomer.Name = customer.Name;
                    originalCustomer.Surname = customer.Surname;
                    originalCustomer.Email = customer.Email;
                    originalCustomer.Username = customer.Username;
                    originalCustomer.ShippingAddress = customer.ShippingAddress; //edit post, so we give it the go ahead to change the values in table using the newly entered ones
                    

                    // Update in Azure Table
                    await _storageService.UpdateEntityAsync(originalCustomer); //updates the customer (saves)

                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating customer:{ex.Message}");
                }
            }
            return View(customer);
        }
        //delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _storageService.DeleteEntityAsync<Customer>("Customer", id);
                TempData["Sucess"] = "Customer deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error delecting customer:{ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
