using Store.Models.Interfaces.Services;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StoreUI.Services
{
    public class ApiFileService : IFileService
    {
        private readonly IApiClientService _client;
        private readonly ILogger<ApiFileService> _logger;

        public ApiFileService(IApiClientService client, ILogger<ApiFileService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<(string? ThumbnailUrl, string? FullImageUrl)> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder, CancellationToken ct = default)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                
                content.Add(fileContent, "file", fileName);

                var endpoint = $"/api/files/upload?folder={System.Uri.EscapeDataString(folder)}";
                
                var result = await _client.PostMultipartAsync<FileUploadResponse>(endpoint, content, ct);
                
                if (result != null && result.Success)
                {
                    return (result.ThumbnailUrl, result.FullImageUrl);
                }
                return (null, null);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
                return (null, null);
            }
        }

        public async Task<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default)
        {
            var endpoint = $"/api/files?relativePath={System.Uri.EscapeDataString(relativePath)}";
            return await _client.DeleteAsync(endpoint, ct);
        }

        private class FileUploadResponse
        {
            public bool Success { get; set; }
            public string? ThumbnailUrl { get; set; }
            public string? FullImageUrl { get; set; }
            public string? Message { get; set; }
        }
    }
}
