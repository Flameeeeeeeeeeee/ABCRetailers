using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Services;
using ABCRetailersFunctions.Models;
using Azure.Storage.Blobs;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly BlobServiceClient _blobService;
        private const string ContainerName = "product-images";

        public ProductController(IFunctionsApi functionsApi, BlobServiceClient blobService)
        {
            _functionsApi = functionsApi;
            _blobService = blobService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _functionsApi.GetProductsAsync();
            return View(products); // Directly pass Functions DTO
        }

        // GET: Products/Create
        public IActionResult Create() => View();

        //// POST: Products/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ProductDto product, IFormFile? imageFile)
        //{
        //    if (!ModelState.IsValid)
        //        return View(product);

        //    try
        //    {
        //        if (imageFile != null && imageFile.Length > 0)
        //            product.ImageUrl = await UploadImageAsync(imageFile);

        //        await _functionsApi.CreateProductAsync(product);
        //        TempData["Success"] = "Product created successfully!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Error creating product: {ex.Message}");
        //        return View(product);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            try
            {
                using var client = new HttpClient();
                using var form = new MultipartFormDataContent();

                // Add text fields
                form.Add(new StringContent(product.ProductName ?? ""), "ProductName");
                form.Add(new StringContent(product.Description ?? ""), "Description");
                form.Add(new StringContent(product.Price.ToString()), "Price");
                form.Add(new StringContent(product.StockAvailable.ToString()), "StockAvailable");

                // Add image file if present
                if (imageFile != null && imageFile.Length > 0)
                {
                    var streamContent = new StreamContent(imageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    form.Add(streamContent, "file", imageFile.FileName);
                }

                // Send to Azure Function
                var response = await client.PostAsync("http://localhost:7251/api/products", form);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error creating product: {error}");
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                return View(product);
            }
        }

        // GET: Products/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _functionsApi.GetProductAsync(id);
            if (product == null) return NotFound();

            return View(product); // Directly pass Functions DTO
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, ProductDto product, IFormFile? imageFile)
        //{
        //    if (id != product.ProductId) return BadRequest();
        //    if (!ModelState.IsValid) return View(product);

        //    try
        //    {
        //        if (imageFile != null && imageFile.Length > 0)
        //            product.ImageUrl = await UploadImageAsync(imageFile);

        //        await _functionsApi.UpdateProductAsync(id, product);
        //        TempData["Success"] = "Product updated successfully!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Error updating product: {ex.Message}");
        //        return View(product);
        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductDto product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return BadRequest();
            if (!ModelState.IsValid) return View(product);

            try
            {
                using var client = new HttpClient();
                using var form = new MultipartFormDataContent();

                // Add text fields
                form.Add(new StringContent(product.ProductName ?? ""), "ProductName");
                form.Add(new StringContent(product.Description ?? ""), "Description");
                form.Add(new StringContent(product.Price.ToString()), "Price");
                form.Add(new StringContent(product.StockAvailable.ToString()), "StockAvailable");

                // Add file if present
                if (imageFile != null && imageFile.Length > 0)
                {
                    var streamContent = new StreamContent(imageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    form.Add(streamContent, "file", imageFile.FileName);
                }

                // Log outgoing request details
                Console.WriteLine($"Sending PUT to Function: ProductId={product.ProductId}, File={(imageFile?.FileName ?? "none")}");

                var response = await client.PutAsync($"http://localhost:7251/api/products/{id}", form);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Function response: {response.StatusCode} - {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", $"Error updating product: {responseBody}");
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Edit controller: {ex}");
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                return View(product);
            }
        }


        // POST: Products/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _functionsApi.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // Upload image to blob storage
        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            var containerClient = _blobService.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = imageFile.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }
    }
}
