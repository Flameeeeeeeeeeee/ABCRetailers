using System.Net.Http;
using System.Text;
using System.Text.Json;
using ABCRetailers.Models;
using ABCRetailersFunctions.Models;
using static System.Net.WebRequestMethods;

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
        // 1. Manually serialize the object to a JSON string
        // This produces the same JSON that PostAsJsonAsync would send.
        var jsonPayload = JsonSerializer.Serialize(order);

        // 2. LOG the JSON payload to check for case issues or unexpected values
        System.Diagnostics.Debug.WriteLine($"JSON BEING SENT: {jsonPayload}");
        // Console.WriteLine(jsonPayload); // Use this if debugging Console/Terminal

        // 3. Create the StringContent object (what PostAsJsonAsync does internally)
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // 4. Send the request using PostAsync (the manual version)
        var response = await _http.PostAsync("api/orders", content);

        // 5. Throw an exception if the status code is not 2xx
        response.EnsureSuccessStatusCode();

        // 6. Read and return the response DTO
        return await response.Content.ReadFromJsonAsync<OrderDto>()!;
    }
    //public async Task<OrderDto> CreateOrderAsync(OrderDto order)
    //{
    //    // In your FunctionsApiClient.CreateOrderAsync method, BEFORE the PostAsync call:
    //    var jsonContent = await content.ReadAsStringAsync();
    //    System.Diagnostics.Debug.WriteLine("JSON BEING SENT: " + jsonContent);
    //    // Or use your preferred logger
    //    var response = await _http.PostAsJsonAsync("api/orders", order);
    //    response.EnsureSuccessStatusCode();
    //    return await response.Content.ReadFromJsonAsync<OrderDto>()!;
    //}


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
        //new
        public async Task<CustomerDto?> GetCustomerByIdAsync(string customerId)
        {
            var response = await _http.GetAsync($"/api/customers/{customerId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CustomerDto>();
            }
            return null;
        }

        public async Task<ProductDto?> GetProductByIdAsync(string productId)
        {
            var response = await _http.GetAsync($"/api/products/{productId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductDto>();
            }
            return null;

        }
    }
}