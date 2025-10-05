using System.Net;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ABCRetailersFunctions.Entities;
using ABCRetailersFunctions.Helpers;
using ABCRetailersFunctions.Models;
using Microsoft.Extensions.Logging;

namespace ABCRetailersFunctions.Functions
{
    public class CustomersFunctions
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger _logger;
        private readonly string _tableName = Environment.GetEnvironmentVariable("TABLE_CUSTOMER") ?? "Customers";

        public CustomersFunctions(TableServiceClient tableServiceClient, ILogger<CustomersFunctions> logger)
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

        [Function("Customers_List")]
        public async Task<HttpResponseData> List(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
        {
            var table = GetTableClient();
            var customers = table.Query<CustomerEntity>().Select(Map.ToDto).ToList();

            var response = req.CreateResponse();
            await response.WriteJsonAsync(customers);
            return response;
        }

        [Function("Customers_Get")]
        public async Task<HttpResponseData> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{id}")] HttpRequestData req,
            string id)
        {
            var response = req.CreateResponse();
            var table = GetTableClient();

            try
            {
                var entity = await table.GetEntityAsync<CustomerEntity>("Customer", id);
                await response.WriteJsonAsync(Map.ToDto(entity.Value));
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteTextAsync("Customer not found");
            }
            return response;
        }

        [Function("Customers_Create")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
        {
            var dto = await HttpJson.ReadJsonAsync<CustomerDto>(req, _logger);
            if (dto == null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var table = GetTableClient();
            var entity = Map.ToEntity(dto);
            await table.AddEntityAsync(entity);

            var response = req.CreateResponse();
            await response.WriteJsonAsync(Map.ToDto(entity), HttpStatusCode.Created);
            return response;
        }

        [Function("Customers_Update")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{id}")] HttpRequestData req,
            string id)
        {
            var dto = await HttpJson.ReadJsonAsync<CustomerDto>(req, _logger);
            if (dto == null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var table = GetTableClient();

            try
            {
                var existing = await table.GetEntityAsync<CustomerEntity>("Customer", id);
                var updated = Map.ToEntity(dto, existing.Value);
                await table.UpdateEntityAsync(updated, existing.Value.ETag, TableUpdateMode.Replace);

                var response = req.CreateResponse();
                await response.WriteJsonAsync(Map.ToDto(updated));
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteTextAsync("Customer not found");
                return response;
            }
        }

        [Function("Customers_Delete")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{id}")] HttpRequestData req,
            string id)
        {
            var table = GetTableClient();

            try
            {
                await table.DeleteEntityAsync("Customer", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteTextAsync("Customer not found");
                return response;
            }
        }
    }
}
