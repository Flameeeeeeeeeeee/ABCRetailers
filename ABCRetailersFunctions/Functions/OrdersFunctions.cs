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
    public class OrdersFunctions
    {
        private readonly TableClient _tableClient;
        private readonly ILogger _logger;

        public OrdersFunctions(TableClient tableClient, ILogger<OrdersFunctions> logger)
        {
            _tableClient = tableClient;
            _logger = logger;
        }

        [Function("Orders_List")]
        public async Task<HttpResponseData> List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequestData req)
        {
            var orders = _tableClient.Query<OrderEntity>().Select(Map.ToDto).ToList();
            var response = req.CreateResponse();
            await response.WriteJsonAsync(orders);
            return response;
        }

        [Function("Orders_Get")]
        public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{id}")] HttpRequestData req, string id)
        {
            var response = req.CreateResponse();
            try
            {
                var entity = await _tableClient.GetEntityAsync<OrderEntity>("Order", id);
                await response.WriteJsonAsync(Map.ToDto(entity.Value));
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteTextAsync("Order not found");
            }
            return response;
        }

        [Function("Orders_Create")]
        public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req)
        {
            var dto = await HttpJson.ReadJsonAsync<OrderDto>(req, _logger);
            if (dto == null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var entity = Map.ToEntity(dto);
            await _tableClient.AddEntityAsync(entity);

            var response = req.CreateResponse();
            await response.WriteJsonAsync(Map.ToDto(entity), HttpStatusCode.Created);
            return response;
        }

        [Function("Orders_Delete")]
        public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{id}")] HttpRequestData req, string id)
        {
            try
            {
                await _tableClient.DeleteEntityAsync("Order", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteTextAsync("Order not found");
                return response;
            }
        }

            [Function("Orders_UpdateStatus")]
            public async Task<HttpResponseData> UpdateStatus([HttpTrigger(AuthorizationLevel.Function, "patch", "post", Route = "orders/{id}/status")] HttpRequestData req, string id)
            {
                var dto = await HttpJson.ReadJsonAsync<OrderDto>(req, _logger);
                if (dto == null || string.IsNullOrEmpty(dto.Status))
                    return req.CreateResponse(HttpStatusCode.BadRequest);

                try
                {
                    var existing = await _tableClient.GetEntityAsync<OrderEntity>("Order", id);
                    existing.Value.Status = dto.Status; // Update status only
                    await _tableClient.UpdateEntityAsync(existing.Value, existing.Value.ETag, TableUpdateMode.Replace);

                    var response = req.CreateResponse();
                    await response.WriteJsonAsync(Map.ToDto(existing.Value));
                    return response;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    var response = req.CreateResponse(HttpStatusCode.NotFound);
                    await response.WriteTextAsync("Order not found");
                    return response;
                }
            }

        }
    }







