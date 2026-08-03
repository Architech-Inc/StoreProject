using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StoreUI.Services;

public class ApiFileService : IFileService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiFileService> _logger;

    public ApiFileService(IApiClientService client, ILogger<ApiFileService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<(string? ThumbnailUrl, string? FullImageUrl)> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder,
        int? cropX = null, int? cropY = null, int? cropW = null, int? cropH = null,
        CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            content.Add(fileContent, "file", fileName);

            var endpoint = $"/api/files/upload?folder={Uri.EscapeDataString(folder)}";

            if (cropX.HasValue && cropY.HasValue && cropW.HasValue && cropH.HasValue)
            {
                endpoint += $"&cropX={cropX.Value}&cropY={cropY.Value}&cropW={cropW.Value}&cropH={cropH.Value}";
            }

            // ApiClientService unwraps ApiResponse<T>.Data -> FileUploadResultDto
            var result = await _client.PostMultipartAsync<FileUploadResultDto>(endpoint, content, ct);

            if (result is null ||
                string.IsNullOrWhiteSpace(result.ThumbnailUrl) ||
                string.IsNullOrWhiteSpace(result.FullImageUrl))
            {
                _logger.LogWarning("File upload returned empty URLs for {FileName}", fileName);
                return (null, null);
            }

            return (result.ThumbnailUrl, result.FullImageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            return (null, null);
        }
    }

    public async Task<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var endpoint = $"/api/files?relativePath={Uri.EscapeDataString(relativePath)}";
        return await _client.DeleteAsync(endpoint, ct);
    }
}
