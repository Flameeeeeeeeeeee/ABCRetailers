using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Models;
using ABCRetailersFunctions.Entities;

namespace ABCRetailersFunctions.Functions
{
    public class QueueProcessorFunctions
    {
        private readonly TableClient _ordersTable;
        private readonly TableClient _productsTable;
        private readonly ILogger _logger;

        public QueueProcessorFunctions(TableClient ordersTable, TableClient productsTable, ILogger<QueueProcessorFunctions> logger)
        {
            _ordersTable = ordersTable;
            _productsTable = productsTable;
            _logger = logger;
        }

        [Function("OrderNotifications_Processor")]
        public async Task ProcessOrderQueue(
            [QueueTrigger("%QueueName%", Connection = "StorageConnectionString")] string message,
            FunctionContext context)
        {
            _logger.LogInformation($"Processing order notification message: {message}");

            try
            {
                var orderDto = JsonSerializer.Deserialize<OrderDto>(message);
                if (orderDto == null)
                {
                    _logger.LogWarning("Message deserialized to null OrderDto");
                    return;
                }

                // Example: Update order status in table or send notifications
                var orderEntityResponse = await _ordersTable.GetEntityAsync<OrderEntity>("Order", orderDto.OrderId!);
                var orderEntity = orderEntityResponse.Value;
                orderEntity.Status = orderDto.Status;
                await _ordersTable.UpdateEntityAsync(orderEntity, orderEntity.ETag, Azure.Data.Tables.TableUpdateMode.Replace);

                _logger.LogInformation($"Order {orderDto.OrderId} status updated to {orderDto.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order notification");
            }
        }

        [Function("StockUpdates_Processor")]
        public async Task ProcessStockQueue(
            [QueueTrigger("stock-updates", Connection = "StorageConnectionString")] string message,
            FunctionContext context)
        {
            _logger.LogInformation($"Processing stock update message: {message}");

            try
            {
                var stockUpdate = JsonSerializer.Deserialize<StockUpdateDto>(message);
                if (stockUpdate == null)
                {
                    _logger.LogWarning("Message deserialized to null StockUpdateDto");
                    return;
                }

                var productEntityResponse = await _productsTable.GetEntityAsync<ProductEntity>("Product", stockUpdate.ProductId);
                var productEntity = productEntityResponse.Value;
                productEntity.StockAvailable = stockUpdate.NewStock;
                await _productsTable.UpdateEntityAsync(productEntity, productEntity.ETag, Azure.Data.Tables.TableUpdateMode.Replace);

                _logger.LogInformation($"Stock updated for Product {stockUpdate.ProductId} to {stockUpdate.NewStock}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock update");
            }
        }
    }
}
