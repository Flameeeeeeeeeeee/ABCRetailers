using System.Security.Cryptography.Xml;
using ABCRetailers.Models;

namespace ABCRetailers.Services
{
    public interface IAzureStorageService
    {
        // table operations
        Task<List<T>> GetAllEntitiesAsync<T>() where T : class, Azure.Data.Tables.ITableEntity, new();
        Task<T?> GetEntityAsync<T>(string partitionkey, string rowkey) where T : class, Azure.Data.Tables.ITableEntity, new();
        Task<T> AddEntityAsync<T>(T entity) where T : class, Azure.Data.Tables.ITableEntity;
       Task<T> UpdateEntityAsync<T>(T entity) where T : class, Azure.Data.Tables.ITableEntity;
        Task DeleteEntityAsync<T>(string partitionkey, string rowkey) where T : class, Azure.Data.Tables.ITableEntity, new();
        //Blob operations
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task DeleteBlobAsync(string blobName, string containerName);
        // Queue operations
        Task SendMessageAsync(string queueName, string message);
        Task<string?> ReceiveMessageAsync(string queueName);
        // File Share operations

        Task<string> UploadToFileShareAsync(IFormFile file, string shareName, string directoryName = "");
        Task<byte[]> DownloadFromFileShareAsync(string shareName, string fileName, string directoryName = "");
    }
}
