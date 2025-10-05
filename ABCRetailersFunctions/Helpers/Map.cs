using ABCRetailersFunctions.Entities;
using ABCRetailersFunctions.Models;

namespace ABCRetailersFunctions.Helpers
{
    public static class Map
    {
        // ---------------- Customers ----------------
        public static CustomerDto ToDto(CustomerEntity entity) => new CustomerDto
        {
            CustomerId = entity.RowKey,
            Name = entity.Name,
            Surname = entity.Surname,
            Username = entity.Username,
            Email = entity.Email,
            ShippingAddress = entity.ShippingAddress,
            ProfilePictureUrl = entity.ProfilePictureUrl
        };

        public static CustomerEntity ToEntity(CustomerDto dto, CustomerEntity? existing = null)
        {
            var entity = existing ?? new CustomerEntity();
            entity.Name = dto.Name;
            entity.Surname = dto.Surname;
            entity.Username = dto.Username;
            entity.Email = dto.Email;
            entity.ShippingAddress = dto.ShippingAddress;
            entity.ProfilePictureUrl = dto.ProfilePictureUrl;
            return entity;
        }


        // ---------------- Products ----------------
        public static ProductDto ToDto(ProductEntity entity) => new()
        {
            ProductId = entity.RowKey,
            ProductName = entity.ProductName,
            Description = entity.Description,
            Price = entity.Price,
            StockAvailable = entity.StockAvailable,
            ImageUrl = entity.ImageUrl
        };

        public static ProductEntity ToEntity(ProductDto dto, ProductEntity? existing = null)
        {
            var entity = existing ?? new ProductEntity();
            entity.ProductName = dto.ProductName;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.StockAvailable = dto.StockAvailable;
            entity.ImageUrl = dto.ImageUrl;
            return entity;
        }



        //// ---------------- Orders ----------------
        //public static OrderDto ToDto(OrderEntity entity) => new()
        //{
        //    OrderId = entity.RowKey,
        //    CustomerId = entity.CustomerId,
        //    ProductId = entity.ProductId,
        //    Quantity = entity.Quantity,
        //    TotalPrice = entity.TotalPrice,
        //    Status = entity.Status,
        //    OrderDate = entity.OrderDate
        //};

        //public static OrderEntity ToEntity(OrderDto dto, OrderEntity? existing = null)
        //{
        //    var entity = existing ?? new OrderEntity();
        //    entity.CustomerId = dto.CustomerId;
        //    entity.ProductId = dto.ProductId;
        //    entity.Quantity = dto.Quantity;
        //    entity.TotalPrice = dto.TotalPrice;
        //    entity.Status = dto.Status;
        //    entity.OrderDate = dto.OrderDate;
        //    return entity;
        //}
        public static OrderDto ToDto(OrderEntity entity) => new()
        {
            OrderId = entity.RowKey,
            CustomerId = entity.CustomerId,
            Username = entity.Username,         // ✅ add this
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,   // ✅ add this
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,       // ✅ add this
            TotalPrice = entity.TotalPrice,
            Status = entity.Status,
            OrderDate = entity.OrderDate
        };

        public static OrderEntity ToEntity(OrderDto dto, OrderEntity? existing = null)
        {
            var entity = existing ?? new OrderEntity();
            entity.CustomerId = dto.CustomerId;
            entity.Username = dto.Username;          // ✅ add this
            entity.ProductId = dto.ProductId;
            entity.ProductName = dto.ProductName;    // ✅ add this
            entity.Quantity = dto.Quantity;
            entity.UnitPrice = dto.UnitPrice;        // ✅ add this
            entity.TotalPrice = dto.TotalPrice;
            entity.Status = dto.Status;
            entity.OrderDate = dto.OrderDate;
            return entity;
        }


    }
}

