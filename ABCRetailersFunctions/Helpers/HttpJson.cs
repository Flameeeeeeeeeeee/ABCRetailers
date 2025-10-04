using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ABCRetailersFunctions.Helpers
{
    public static class HttpJson
    {
        // Read JSON body and deserialize into T
        public static async Task<T?> ReadJsonAsync<T>(HttpRequestData req, ILogger? logger = null)
        {
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(req.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to deserialize JSON request body");
                return default;
            }
        }


        // Write JSON to response
        public static async Task WriteJsonAsync<T>(this HttpResponseData resp, T obj, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            resp.StatusCode = statusCode;
            resp.Headers.Add("Content-Type", "application/json");
            await resp.WriteStringAsync(JsonSerializer.Serialize(obj));
        }

        // Write plain text
        public static async Task WriteTextAsync(this HttpResponseData resp, string text, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            resp.StatusCode = statusCode;
            resp.Headers.Add("Content-Type", "text/plain");
            await resp.WriteStringAsync(text);
        }
    }
}
