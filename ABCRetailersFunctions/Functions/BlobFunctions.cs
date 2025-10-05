using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Entities;

namespace ABCRetailersFunctions.Functions
{
    public class BlobFunctions
    {
        private readonly TableClient _productsTable;
        private readonly ILogger _logger;

        public BlobFunctions(TableClient productsTable, ILogger<BlobFunctions> logger)
        {
            _productsTable = productsTable;
            _logger = logger;
        }

        [Function("OnProductImageUpload")]
        public async Task OnProductImageUpload(
            [BlobTrigger("%BLOB_PRODUCT_IMAGES%/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
            string name)
        {
            _logger.LogInformation($"Blob trigger fired for: {name}, size: {blobStream.Length} bytes");

            try
            {
                var imageUrl = $"https://tameezabcretailers.blob.core.windows.net/product-images/{name}";
                var productId = Path.GetFileNameWithoutExtension(name);

                var entityResponse = await _productsTable.GetEntityAsync<ProductEntity>("Product", productId);
                var entity = entityResponse.Value;

                entity.ImageUrl = imageUrl;
                await _productsTable.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

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
