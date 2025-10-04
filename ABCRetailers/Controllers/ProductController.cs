//using Microsoft.AspNetCore.Mvc;
//using ABCRetailers.Services;

//using ABCRetailersFunctions.Models; // Functions API DTO
//using Azure.Storage.Blobs;

//namespace ABCRetailers.Controllers
//{
//    public class ProductController : Controller
//    {
//        private readonly IFunctionsApi _functionsApi;
//        private readonly BlobServiceClient _blobService;

//        private const string ContainerName = "product-images";

//        public ProductController(IFunctionsApi functionsApi, BlobServiceClient blobService)
//        {
//            _functionsApi = functionsApi;
//            _blobService = blobService;
//        }

//        // GET: Products
//        public async Task<IActionResult> Index()
//        {
//            var apiProducts = await _functionsApi.GetProductsAsync(); // Functions DTO
//            // Map to MVC DTO for the view
//            var products = apiProducts.Select(p => new ProductDto
//            {
//                ProductId = p.ProductId,
//                ProductName = p.ProductName,
//                Description = p.Description,
//                Price = p.Price,
//                StockAvailable = p.StockAvailable,
//                ImageUrl = p.ImageUrl
//            });
//            return View(products);
//        }

//        // GET: Products/Create
//        public IActionResult Create() => View();

//        // POST: Products/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(ProductDto product, IFormFile? imageFile)
//        {
//            if (!ModelState.IsValid) return View(product);

//            try
//            {
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    product.ImageUrl = await UploadImageAsync(imageFile);
//                }

//                // Map MVC DTO -> Functions DTO
//                var apiDto = new ProductDto
//                {
//                    ProductId = product.ProductId,
//                    ProductName = product.ProductName,
//                    Description = product.Description,
//                    Price = product.Price,
//                    StockAvailable = product.StockAvailable,
//                    ImageUrl = product.ImageUrl
//                };

//                await _functionsApi.CreateProductAsync(apiDto);
//                TempData["Success"] = "Product created successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
//                return View(product);
//            }
//        }

//        // GET: Products/Edit/{id}
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (string.IsNullOrEmpty(id)) return NotFound();

//            var apiProduct = await _functionsApi.GetProductAsync(id);
//            if (apiProduct == null) return NotFound();

//            // Map Functions DTO -> MVC DTO
//            var product = new ProductDto
//            {
//                ProductId = apiProduct.ProductId,
//                ProductName = apiProduct.ProductName,
//                Description = apiProduct.Description,
//                Price = apiProduct.Price,
//                StockAvailable = apiProduct.StockAvailable,
//                ImageUrl = apiProduct.ImageUrl
//            };

//            return View(product);
//        }

//        // POST: Products/Edit/{id}
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(string id, ProductDto product, IFormFile? imageFile)
//        {
//            if (id != product.ProductId) return BadRequest();

//            if (!ModelState.IsValid) return View(product);

//            try
//            {
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    product.ImageUrl = await UploadImageAsync(imageFile);
//                }

//                var apiDto = new ProductDto
//                {
//                    ProductId = product.ProductId,
//                    ProductName = product.ProductName,
//                    Description = product.Description,
//                    Price = product.Price,
//                    StockAvailable = product.StockAvailable,
//                    ImageUrl = product.ImageUrl
//                };

//                await _functionsApi.UpdateProductAsync(id, apiDto);
//                TempData["Success"] = "Product updated successfully!";
//                return RedirectToAction(nameof(Index));
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
//                return View(product);
//            }
//        }

//        // POST: Products/Delete/{id}
//        [HttpPost]
//        public async Task<IActionResult> Delete(string id)
//        {
//            try
//            {
//                await _functionsApi.DeleteProductAsync(id);
//                TempData["Success"] = "Product deleted successfully!";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Error deleting product: {ex.Message}";
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        // ---------------- Helper for Blob Upload ----------------
//        private async Task<string> UploadImageAsync(IFormFile imageFile)
//        {
//            var containerClient = _blobService.GetBlobContainerClient(ContainerName);
//            await containerClient.CreateIfNotExistsAsync();
//            var blobName = $"{Guid.NewGuid()}_{imageFile.FileName}";
//            var blobClient = containerClient.GetBlobClient(blobName);

//            using var stream = imageFile.OpenReadStream();
//            await blobClient.UploadAsync(stream, overwrite: true);

//            return blobClient.Uri.ToString();
//        }
//    }
//}
