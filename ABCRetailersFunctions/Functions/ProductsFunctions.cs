using System.Net;
using ABCRetailersFunctions.Entities;
using ABCRetailersFunctions.Helpers;
using ABCRetailersFunctions.Models;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetailersFunctions.Functions
{
    public class ProductsFunctions
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger _logger;
        private readonly string _tableName = Environment.GetEnvironmentVariable("TABLE_PRODUCT") ?? "Products";

        public ProductsFunctions(TableServiceClient tableServiceClient, ILogger<ProductsFunctions> logger)
        {
            _tableServiceClient = tableServiceClient;
            _logger = logger;
        }

        private TableClient GetTableClient()
        {
            var tableClient = _tableServiceClient.GetTableClient(_tableName);
            tableClient.CreateIfNotExists();
            return tableClient;
        }

        [Function("Products_List")]
        public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequestData req)
        {
            var table = GetTableClient();
            var products = table.Query<ProductEntity>().Select(Map.ToDto).ToList();

            var response = req.CreateResponse();
            await response.WriteJsonAsync(products);
            return response;
        }

        [Function("Products_Get")]
        public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id}")] HttpRequestData req, string id)
        {
            var response = req.CreateResponse();
            var table = GetTableClient();

            try
            {
                var entity = await table.GetEntityAsync<ProductEntity>("Product", id);
                await response.WriteJsonAsync(Map.ToDto(entity.Value));
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteTextAsync("Product not found");
            }

            return response;
        }
        [Function("Products_Create")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestData req)
        {
            ProductDto dto;

            // Blob service setup
            var blobService = new BlobServiceClient(Environment.GetEnvironmentVariable("StorageConnectionString"));
            var containerName = "product-images";
            var containerClient = blobService.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            if (MultipartHelper.IsMultipartContentType(req.Headers))
            {
                // Handle multipart/form-data
                var multipartData = await MultipartHelper.ReadMultipartAsync(req);
                _logger.LogInformation("FormFields: {@fields}", multipartData.FormFields);
                _logger.LogInformation("Files: {@files}", multipartData.Files.Select(f => f.FileName));

                var formFields = multipartData.FormFields;
                var file = multipartData.Files.FirstOrDefault();

                dto = new ProductDto
                {
                    ProductName = formFields.GetValueOrDefault("ProductName", string.Empty),
                    Description = formFields.GetValueOrDefault("Description", string.Empty),
                    Price = double.TryParse(formFields.GetValueOrDefault("Price"), out var p) ? p : 0,
                    StockAvailable = int.TryParse(formFields.GetValueOrDefault("StockAvailable"), out var s) ? s : 0
                };

                if (file != null)
                {
                    var blobName = $"{Guid.NewGuid()}_{file.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);

                    dto.ImageUrl = blobClient.Uri.ToString();
                }
            }
            else
            {
                // Assume JSON
                try
                {
                    dto = await HttpJson.ReadJsonAsync<ProductDto>(req, _logger);
                    if (dto == null)
                    {
                        var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badResp.WriteTextAsync("Invalid JSON body");
                        return badResp;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON request body");
                    var errorResp = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResp.WriteTextAsync("Invalid JSON format");
                    return errorResp;
                }
            }

            // Save to table
            var table = GetTableClient();
            var entity = Map.ToEntity(dto);
            await table.AddEntityAsync(entity);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteJsonAsync(Map.ToDto(entity));
            return response;
        }




        //old
        //[Function("Products_Create")]
        //public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestData req)
        //{
        //    var dto = await HttpJson.ReadJsonAsync<ProductDto>(req, _logger);
        //    if (dto == null) return req.CreateResponse(HttpStatusCode.BadRequest);

        //    var table = GetTableClient();
        //    var entity = Map.ToEntity(dto);
        //    await table.AddEntityAsync(entity);

        //    var response = req.CreateResponse();
        //    await response.WriteJsonAsync(Map.ToDto(entity), HttpStatusCode.Created);
        //    return response;
        //}




        //    [Function("Products_Update")]
        //    public async Task<HttpResponseData> Update(
        //[HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id}")] HttpRequestData req,
        //string id)
        //    {
        //        ProductDto dto;

        //        // Blob service setup
        //        var blobService = new BlobServiceClient(Environment.GetEnvironmentVariable("StorageConnectionString"));
        //        var containerName = "product-images";
        //        var containerClient = blobService.GetBlobContainerClient(containerName);
        //        await containerClient.CreateIfNotExistsAsync();

        //        if (MultipartHelper.IsMultipartContentType(req.Headers))
        //        {
        //            // Handle multipart/form-data
        //            var multipartData = await MultipartHelper.ReadMultipartAsync(req);

        //            var formFields = multipartData.FormFields;
        //            var file = multipartData.Files.FirstOrDefault();

        //            dto = new ProductDto
        //            {
        //                ProductName = formFields.GetValueOrDefault("ProductName", string.Empty),
        //                Description = formFields.GetValueOrDefault("Description", string.Empty),
        //                Price = double.TryParse(formFields.GetValueOrDefault("Price"), out var p) ? p : 0,
        //                StockAvailable = int.TryParse(formFields.GetValueOrDefault("StockAvailable"), out var s) ? s : 0

        //            };

        //            if (file != null)
        //            {
        //                var blobName = $"{Guid.NewGuid()}_{file.FileName}";
        //                var blobClient = containerClient.GetBlobClient(blobName);

        //                using var stream = file.OpenReadStream();
        //                await blobClient.UploadAsync(stream, overwrite: true);

        //                dto.ImageUrl = blobClient.Uri.ToString();
        //            }
        //        }
        //        else
        //        {
        //            // Assume JSON
        //            try
        //            {
        //                dto = await HttpJson.ReadJsonAsync<ProductDto>(req, _logger);
        //                if (dto == null)
        //                {
        //                    var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
        //                    await badResp.WriteTextAsync("Invalid JSON body");
        //                    return badResp;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Failed to parse JSON request body");
        //                var errorResp = req.CreateResponse(HttpStatusCode.BadRequest);
        //                await errorResp.WriteTextAsync("Invalid JSON format");
        //                return errorResp;
        //            }
        //        }

        //        // Update table
        //        var table = GetTableClient();
        //        try
        //        {
        //            var existing = await table.GetEntityAsync<ProductEntity>("Product", id);
        //            var updated = Map.ToEntity(dto, existing.Value);
        //            if(string.IsNullOrEmpty(dto.ImageUrl))
        //{
        //                updated.ImageUrl = existing.Value.ImageUrl;
        //            }

        //            await table.UpdateEntityAsync(updated, existing.Value.ETag, TableUpdateMode.Replace);

        //            var response = req.CreateResponse();
        //            await response.WriteJsonAsync(Map.ToDto(updated));
        //            return response;
        //        }
        //        catch (RequestFailedException ex) when (ex.Status == 404)
        //        {
        //            var response = req.CreateResponse(HttpStatusCode.NotFound);
        //            await response.WriteTextAsync("Product not found");
        //            return response;
        //        }
        //    }

        [Function("Products_Update")]
        public async Task<HttpResponseData> Update(
    [HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id}")] HttpRequestData req,
    string id)
        {
            ProductDto dto;

            var table = GetTableClient();
            var existing = await table.GetEntityAsync<ProductEntity>("Product", id);

            // Blob service setup
            var blobService = new BlobServiceClient(Environment.GetEnvironmentVariable("StorageConnectionString"));
            var containerName = "product-images";
            var containerClient = blobService.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            if (MultipartHelper.IsMultipartContentType(req.Headers))
            {
                var multipartData = await MultipartHelper.ReadMultipartAsync(req);
                var formFields = multipartData.FormFields;
                var file = multipartData.Files.FirstOrDefault();

                dto = new ProductDto
                {
                    ProductName = formFields.GetValueOrDefault("ProductName", string.Empty),
                    Description = formFields.GetValueOrDefault("Description", string.Empty),
                    Price = double.TryParse(formFields.GetValueOrDefault("Price"), out var p) ? p : 0,
                    StockAvailable = int.TryParse(formFields.GetValueOrDefault("StockAvailable"), out var s) ? s : 0,
                    ImageUrl = file != null ? null : existing.Value.ImageUrl // now existing is defined
                };

                if (file != null)
                {
                    var blobName = $"{Guid.NewGuid()}_{file.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);

                    dto.ImageUrl = blobClient.Uri.ToString();
                }
            }
            else
            {
                try
                {
                    dto = await HttpJson.ReadJsonAsync<ProductDto>(req, _logger);
                    if (dto == null)
                    {
                        var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badResp.WriteTextAsync("Invalid JSON body");
                        return badResp;
                    }

                    // Preserve existing image if JSON body has no ImageUrl
                    if (string.IsNullOrEmpty(dto.ImageUrl))
                    {
                        dto.ImageUrl = existing.Value.ImageUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON request body");
                    var errorResp = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResp.WriteTextAsync("Invalid JSON format");
                    return errorResp;
                }
            }

            // Map and update table
            var updated = Map.ToEntity(dto, existing.Value);
            await table.UpdateEntityAsync(updated, existing.Value.ETag, TableUpdateMode.Replace);

            var response = req.CreateResponse();
            await response.WriteJsonAsync(Map.ToDto(updated));
            return response;
        }

        //delete
        [Function("Products_Delete")]
        public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id}")] HttpRequestData req, string id)
        {
            var table = GetTableClient();

            try
            {
                await table.DeleteEntityAsync("Product", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteTextAsync("Product not found");
                return response;
            }
        }
    }
}
