using System.Security.Cryptography.Xml;
using ABCRetailers.Models;
using ABCRetailersFunctions.Models;


namespace ABCRetailers.Services
{
    public interface IFunctionsApi
    {
        // Customers
        Task<List<CustomerDto>> GetCustomersAsync();
        Task<CustomerDto?> GetCustomerAsync(string id);
        Task CreateCustomerAsync(CustomerDto customer);
        Task UpdateCustomerAsync(string id, CustomerDto customer);
        Task DeleteCustomerAsync(string id);

        // Products
        Task<IEnumerable<ProductDto>> GetProductsAsync();
        Task<ProductDto?> GetProductAsync(string id);
        Task<ProductDto> CreateProductAsync(ProductDto product);
        Task<ProductDto> UpdateProductAsync(string id, ProductDto product);
        Task<bool> DeleteProductAsync(string id);

        // Orders
        Task<IEnumerable<OrderDto>> GetOrdersAsync();
        Task<OrderDto?> GetOrderAsync(string id);
        Task<OrderDto> CreateOrderAsync(OrderDto order);
        Task<OrderDto> UpdateOrderStatusAsync(string id, string status);
        Task<bool> DeleteOrderAsync(string id);
        //new
        Task<CustomerDto?> GetCustomerByIdAsync(string customerId);
        Task<ProductDto?> GetProductByIdAsync(string productId);

        //uploads
        Task<string> UploadProofOfPaymentAsync(IFormFile file, string? orderId, string? customerName);
    }

}

