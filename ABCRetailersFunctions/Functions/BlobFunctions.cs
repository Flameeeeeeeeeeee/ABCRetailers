using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Entities;
using ABCRetailersFunctions.Helpers;
using ABCRetailersFunctions.Models;

namespace ABCRetailersFunctions.Functions
{
    public class BlobFunctions
    {
        private readonly TableClient _tableClient;
        private readonly ILogger _logger;

        public BlobFunctions(TableClient tableClient, ILogger<BlobFunctions> logger)
        {
            _tableClient = tableClient;
            _logger = logger;
        }

        [Function("OnProductImageUpload")]
        public async Task OnProductImageUpload(
            [BlobTrigger("product-images/{name}", Connection = "StorageConnectionString")] Stream blobStream,
            string name)
        {
            _logger.LogInformation($"Blob trigger fired for: {name}, size: {blobStream.Length} bytes");

            try
            {
                // Generate a URL or blob path (simplified)
                var imageUrl = $"https://tameezabcretailers.blob.core.windows.net/product-images/{name}";

                // Fetch the corresponding ProductEntity by ProductId
                var productId = Path.GetFileNameWithoutExtension(name); // assume blob name = ProductId.ext
                var entityResponse = await _tableClient.GetEntityAsync<ProductEntity>("Product", productId);
                var entity = entityResponse.Value;

                // Update ImageUrl
                entity.ImageUrl = imageUrl;

                await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

                _logger.LogInformation($"Updated Product {productId} with image URL: {imageUrl}");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"Product not found for uploaded image: {name}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing blob upload");
            }
        }
    }
}
