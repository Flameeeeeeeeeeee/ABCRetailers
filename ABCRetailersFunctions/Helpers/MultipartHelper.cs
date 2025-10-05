//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.WebUtilities;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Net.Http.Headers;

//namespace ABCRetailersFunctions.Helpers
//{
//    public static class MultipartHelper
//    {
//        // 1️⃣ Check if request is multipart/form-data
//        public static bool IsMultipartContentType(HttpHeadersCollection headers)
//        {
//            if (!headers.TryGetValues("Content-Type", out var values)) return false;
//            var contentType = values.FirstOrDefault();
//            return !string.IsNullOrEmpty(contentType) &&
//                   contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
//        }

//        // 2️⃣ Get boundary from Content-Type header
//        public static string GetBoundary(HttpHeadersCollection headers, int lengthLimit = 70)
//        {
//            if (!headers.TryGetValues("Content-Type", out var values))
//                throw new InvalidOperationException("Missing Content-Type header.");

//            var contentType = values.FirstOrDefault();
//            var mediaType = MediaTypeHeaderValue.Parse(contentType);
//            var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

//            if (string.IsNullOrWhiteSpace(boundary))
//                throw new InvalidOperationException("Missing boundary.");

//            if (boundary.Length > lengthLimit)
//                throw new InvalidOperationException($"Multipart boundary length limit {lengthLimit} exceeded.");

//            return boundary;
//        }

//        // 3️⃣ Extract all form fields (ignores files)
//        public static async Task<Dictionary<string, string>> GetFormFields(HttpRequestData req)
//        {
//            var boundary = GetBoundary(req.Headers);
//            var reader = new MultipartReader(boundary, req.Body);

//            var formFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
//            MultipartSection? section;

//            while ((section = await reader.ReadNextSectionAsync()) != null)
//            {
//                var contentDisposition = section.GetContentDispositionHeader();
//                if (contentDisposition.IsFormDisposition())
//                {
//                    using var streamReader = new StreamReader(section.Body);
//                    var value = await streamReader.ReadToEndAsync();
//                    formFields[contentDisposition.Name.Value!] = value;
//                }
//            }

//            return formFields;
//        }

//        // 4️⃣ Get the first file in a multipart request
//        public static async Task<IFormFile?> GetFile(HttpRequestData req)
//        {
//            var boundary = GetBoundary(req.Headers);
//            var reader = new MultipartReader(boundary, req.Body);

//            MultipartSection? section;
//            while ((section = await reader.ReadNextSectionAsync()) != null)
//            {
//                var contentDisposition = section.GetContentDispositionHeader();
//                if (contentDisposition.IsFileDisposition())
//                {
//                    var fileName = contentDisposition.FileName.Value;
//                    var memoryStream = new MemoryStream();
//                    await section.Body.CopyToAsync(memoryStream);
//                    memoryStream.Position = 0;

//                    return new FormFile(memoryStream, 0, memoryStream.Length, contentDisposition.Name.Value, fileName);
//                }
//            }

//            return null; // No file uploaded
//        }
//    }

//    // 5️⃣ Extension methods for ContentDispositionHeaderValue
//    public static class ContentDispositionHeaderValueExtensions
//    {
//        public static bool IsFileDisposition(this ContentDispositionHeaderValue header)
//        {
//            return header != null &&
//                   header.DispositionType.Equals("form-data") &&
//                   (header.FileName.HasValue || header.FileNameStar.HasValue);
//        }

//        public static bool IsFormDisposition(this ContentDispositionHeaderValue header)
//        {
//            return header != null &&
//                   header.DispositionType.Equals("form-data") &&
//                   !header.FileName.HasValue &&
//                   !header.FileNameStar.HasValue;
//        }
//    }
//}



using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Net.Http.Headers;

namespace ABCRetailersFunctions.Helpers
{
    public class MultipartData
    {
        public Dictionary<string, string> FormFields { get; set; } = new();
        public List<IFormFile> Files { get; set; } = new();
    }

    public static class MultipartHelper
    {
        public static bool IsMultipartContentType(HttpHeadersCollection headers)
        {
            if (!headers.TryGetValues("Content-Type", out var values)) return false;
            var contentType = values.FirstOrDefault();
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetBoundary(HttpHeadersCollection headers, int lengthLimit = 70)
        {
            if (!headers.TryGetValues("Content-Type", out var values))
                throw new InvalidOperationException("Missing Content-Type header.");

            var contentType = values.FirstOrDefault();
            var mediaType = MediaTypeHeaderValue.Parse(contentType);
            var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
                throw new InvalidOperationException("Missing boundary.");

            if (boundary.Length > lengthLimit)
                throw new InvalidOperationException($"Multipart boundary length limit {lengthLimit} exceeded.");

            return boundary;
        }

        //  Read stream once and return both form fields & files
        public static async Task<MultipartData> ReadMultipartAsync(HttpRequestData req)
        {
            var boundary = GetBoundary(req.Headers);
            var reader = new MultipartReader(boundary, req.Body);

            var result = new MultipartData();
            MultipartSection? section;

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var contentDisposition = section.GetContentDispositionHeader();
                if (contentDisposition.IsFormDisposition())
                {
                    using var sr = new StreamReader(section.Body);
                    var value = await sr.ReadToEndAsync();
                    result.FormFields[contentDisposition.Name.Value!] = value;
                }
                else if (contentDisposition.IsFileDisposition())
                {
                    var memoryStream = new MemoryStream();
                    await section.Body.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var file = new FormFile(memoryStream, 0, memoryStream.Length,
                        contentDisposition.Name.Value, contentDisposition.FileName.Value);

                    result.Files.Add(file);
                }
            }

            return result;
        }
    }

    public static class ContentDispositionHeaderValueExtensions
    {
        public static bool IsFileDisposition(this ContentDispositionHeaderValue header)
        {
            return header != null &&
                   header.DispositionType.Equals("form-data") &&
                   (header.FileName.HasValue || header.FileNameStar.HasValue);
        }

        public static bool IsFormDisposition(this ContentDispositionHeaderValue header)
        {
            return header != null &&
                   header.DispositionType.Equals("form-data") &&
                   !header.FileName.HasValue &&
                   !header.FileNameStar.HasValue;
        }
    }
}
