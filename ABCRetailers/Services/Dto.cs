using System.Text.Json.Serialization;

namespace ABCRetailers.Services.Dto
{
    // ---------------- Customers ----------------
    public class CustomerDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }

    // ---------------- Products ----------------
    public class ProductDto
    {
        public string? ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockAvailable { get; set; }
        public string? ImageUrl { get; set; }
    }

    // ---------------- Orders ----------------
    public class OrderDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Today;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; } = "Submitted";
    }

    // ---------------- File Upload (optional) ----------------
    public class FileUploadDto
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? FileUrl { get; set; }
    }

    // ---------------- Stock Updates ----------------
    public class StockUpdateDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int NewStock { get; set; }
    }
}
