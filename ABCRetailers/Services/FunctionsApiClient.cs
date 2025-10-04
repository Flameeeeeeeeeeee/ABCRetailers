using System.Text;
using ABCRetailers.Models;
using System.Text.Json;
using ABCRetailersFunctions.Models;

namespace ABCRetailers.Services
{
    public class FunctionsApiClient : IFunctionsApi
    {
        private readonly HttpClient _http;

        public FunctionsApiClient(HttpClient http)
        {
            _http = http;
        }

        // ---------------- Customers ----------------
        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            var dtos = await _http.GetFromJsonAsync<List<CustomerDto>>("api/customers");
            return dtos ?? new List<CustomerDto>();
        }

        public async Task<CustomerDto?> GetCustomerAsync(string id)
        {
            return await _http.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");
        }

        public async Task CreateCustomerAsync(CustomerDto customer)
        {
            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/customers", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateCustomerAsync(string id, CustomerDto customer)
        {
            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"api/customers/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCustomerAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/customers/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ---------------- Products ----------------
        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            var dtos = await _http.GetFromJsonAsync<List<ProductDto>>("api/products");
            return dtos ?? Enumerable.Empty<ProductDto>();
        }

        public async Task<ProductDto?> GetProductAsync(string id)
            => await _http.GetFromJsonAsync<ProductDto>($"api/products/{id}");

        public async Task<ProductDto> CreateProductAsync(ProductDto product)
        {
            var response = await _http.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>()!;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/products/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<ProductDto> UpdateProductAsync(string id, ProductDto product)
        {
            var response = await _http.PutAsJsonAsync($"api/products/{id}", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>()!;
        }

        // ---------------- Orders ----------------
        public async Task<IEnumerable<OrderDto>> GetOrdersAsync()
        {
            var dtos = await _http.GetFromJsonAsync<List<OrderDto>>("api/orders");
            return dtos ?? Enumerable.Empty<OrderDto>();
        }

        public async Task<OrderDto?> GetOrderAsync(string id)
            => await _http.GetFromJsonAsync<OrderDto>($"api/orders/{id}");

        public async Task<OrderDto> CreateOrderAsync(OrderDto order)
        {
            var response = await _http.PostAsJsonAsync("api/orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderDto>()!;
        }

        public async Task<bool> DeleteOrderAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/orders/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(string id, string status)
        {
            var response = await _http.PatchAsJsonAsync($"api/orders/{id}/status", new { Status = status });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderDto>()!;
        }

        // ---------------- Mapping Helpers ----------------
        private static Customer MapToCustomer(CustomerDto dto) => new Customer
        {
            RowKey = dto.CustomerId,
            PartitionKey = "Customer",
            Name = dto.Name,
            Surname = dto.Surname,
            Username = dto.Username,
            Email = dto.Email,
            ShippingAddress = dto.ShippingAddress,
            ProfilePictureUrl = dto.ProfilePictureUrl
        };

        private static CustomerDto MapToCustomerDto(Customer c) => new CustomerDto
        {
            CustomerId = c.RowKey,
            Name = c.Name,
            Surname = c.Surname,
            Username = c.Username,
            Email = c.Email,
            ShippingAddress = c.ShippingAddress,
            ProfilePictureUrl = c.ProfilePictureUrl
        };
    }
}