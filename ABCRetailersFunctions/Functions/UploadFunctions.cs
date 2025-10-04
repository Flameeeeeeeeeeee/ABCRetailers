using System.IO;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Entities;
using ABCRetailersFunctions.Helpers;

namespace ABCRetailersFunctions.Functions
{
    public class UploadsFunctions
    {
        private readonly TableClient _productsTable;
        private readonly ILogger _logger;

        public UploadsFunctions(TableClient productsTable, ILogger<UploadsFunctions> logger)
        {
            _productsTable = productsTable;
            _logger = logger;
        }

        [Function("Uploads_OnProductImageUpload")]
        public async Task Run(
            [BlobTrigger("%BlobContainerName%/{name}", Connection = "StorageConnectionString")] Stream blobStream,
            string name,
            FunctionContext context)
        {
            var logger = context.GetLogger("OnProductImageUpload");
            logger.LogInformation($"New blob uploaded: {name}");

            // Example: generate public URL
            var imageUrl = $"https://<your-storage-account>.blob.core.windows.net/product-images/{name}";

            // Optional: update Table Storage with image URL
            try
            {
                var productEntityResponse = await _productsTable.GetEntityAsync<ProductEntity>("Product", name); // Assuming blob name = ProductId
                var productEntity = productEntityResponse.Value;
                productEntity.ImageUrl = imageUrl;
                await _productsTable.UpdateEntityAsync(productEntity, productEntity.ETag, Azure.Data.Tables.TableUpdateMode.Replace);

                logger.LogInformation($"Updated product {name} with image URL.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update product with image URL");
            }
        }
    }
}
