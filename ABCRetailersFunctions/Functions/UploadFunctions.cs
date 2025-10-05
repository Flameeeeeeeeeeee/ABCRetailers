//using System.Net;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Extensions.Logging;
//using ABCRetailersFunctions.Helpers;
//using Azure.Storage.Blobs;

//namespace ABCRetailersFunctions.Functions
//{
//    public class UploadsFunctions
//    {
//        private readonly BlobServiceClient _blobServiceClient;
//        private readonly ILogger _logger;
//        private readonly string _blobContainerName = Environment.GetEnvironmentVariable("BLOB_PAYMENT_PROOFS") ?? "payment-proofs";

//        public UploadsFunctions(BlobServiceClient blobServiceClient, ILogger<UploadsFunctions> logger)
//        {
//            _blobServiceClient = blobServiceClient;
//            _logger = logger;
//        }

//        [Function("Uploads_ProofOfPayment")]
//        public async Task<HttpResponseData> UploadProofOfPayment([HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploads/proof-of-payment")] HttpRequestData req)
//        {
//            if (!MultipartHelper.IsMultipartContentType(req.Headers))
//            {
//                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
//                await badResp.WriteTextAsync("Expected multipart/form-data content");
//                return badResp;
//            }

//            var file = await MultipartHelper.GetFile(req);
//            if (file == null)
//            {
//                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
//                await badResp.WriteTextAsync("No file uploaded");
//                return badResp;
//            }

//            try
//            {
//                var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
//                await containerClient.CreateIfNotExistsAsync();

//                var blobClient = containerClient.GetBlobClient(file.FileName);
//                file.OpenReadStream().Position = 0;
//                await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);

//                var response = req.CreateResponse(HttpStatusCode.Created);
//                await response.WriteJsonAsync(new { fileName = file.FileName, url = blobClient.Uri.ToString() });
//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to upload proof of payment");
//                var errorResp = req.CreateResponse(HttpStatusCode.InternalServerError);
//                await errorResp.WriteTextAsync("Upload failed");
//                return errorResp;
//            }
//        }
//    }
//}


using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Helpers;
using Azure.Storage.Blobs;

namespace ABCRetailersFunctions.Functions
{
    public class UploadsFunctions
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger _logger;
        private readonly string _blobContainerName = Environment.GetEnvironmentVariable("BLOB_PAYMENT_PROOFS") ?? "payment-proofs";

        public UploadsFunctions(BlobServiceClient blobServiceClient, ILogger<UploadsFunctions> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        [Function("Uploads_ProofOfPayment")]
        public async Task<HttpResponseData> UploadProofOfPayment(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "uploads/proof-of-payment")] HttpRequestData req)
        {
            if (!MultipartHelper.IsMultipartContentType(req.Headers))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteTextAsync("Expected multipart/form-data content");
                return badResp;
            }

            // ✅ Use the new helper
            var multipartData = await MultipartHelper.ReadMultipartAsync(req);
            var file = multipartData.Files.FirstOrDefault();

            if (file == null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteTextAsync("No file uploaded");
                return badResp;
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(file.FileName);
                file.OpenReadStream().Position = 0;
                await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteJsonAsync(new { fileName = file.FileName, url = blobClient.Uri.ToString() });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload proof of payment");
                var errorResp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResp.WriteTextAsync("Upload failed");
                return errorResp;
            }
        }
    }
}