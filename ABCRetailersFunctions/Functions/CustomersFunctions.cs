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
        private readonly TableClient _tableClient;
        private readonly ILogger _logger;

        public CustomersFunctions(TableClient tableClient, ILogger<CustomersFunctions> logger)
        {
            _tableClient = tableClient;
            _logger = logger;
        }

        [Function("Customers_List")]
        public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
        {
            var customers = _tableClient.Query<CustomerEntity>().Select(Map.ToDto).ToList();
            var response = req.CreateResponse();
            await response.WriteJsonAsync(customers);
            return response;
        }

        [Function("Customers_Get")]
        public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{id}")] HttpRequestData req, string id)
        {
            var response = req.CreateResponse();
            try
            {
                var entity = await _tableClient.GetEntityAsync<CustomerEntity>("Customer", id);
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
        public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
        {
            var dto = await HttpJson.ReadJsonAsync<CustomerDto>(req, _logger);
            if (dto == null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var entity = Map.ToEntity(dto);
            await _tableClient.AddEntityAsync(entity);

            var response = req.CreateResponse();
            await response.WriteJsonAsync(Map.ToDto(entity), HttpStatusCode.Created);
            return response;
        }

        [Function("Customers_Update")]
        public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{id}")] HttpRequestData req, string id)
        {
            var dto = await HttpJson.ReadJsonAsync<CustomerDto>(req, _logger);
            if (dto == null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            try
            {
                var existing = await _tableClient.GetEntityAsync<CustomerEntity>("Customer", id);
                var updated = Map.ToEntity(dto, existing.Value);
                await _tableClient.UpdateEntityAsync(updated, existing.Value.ETag, TableUpdateMode.Replace);

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
        public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{id}")] HttpRequestData req, string id)
        {
            try
            {
                await _tableClient.DeleteEntityAsync("Customer", id);
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
