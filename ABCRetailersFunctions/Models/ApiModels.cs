namespace ABCRetailersFunctions.Models
{
    //Customers 
    public class CustomerDto
    {
        public string CustomerId { get; set; } = string.Empty; // maps RowKey
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }

    //  Products 
    public class ProductDto
    {
        public string? ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockAvailable { get; set; }
        public string? ImageUrl { get; set; } // Blob URL after upload
    }
    // ---------------- Orders ----------------
    public class OrderDto
    {
        public string? OrderId { get; set; }      // Maps to RowKey
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

    public enum OrderStatus
    {
        Submitted,   // When order is first created
        Processing,  // When company opens/reviews the order
        Completed,   // When order is delivered to customer
        Cancelled    // When order is cancelled
    }
    public class FileUploadDto
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? FileUrl { get; set; } // This will store the blob/file URL after upload
    }
    
    
        public class StockUpdateDto
        {
            public string ProductId { get; set; } = string.Empty; // ID of the product to update
            public int NewStock { get; set; }                     // Updated stock quantity
        }
    }


