using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.API.Infrastructure.Processing;
using Store.API.Infrastructure.Storage;
using Store.Models.DTOs.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FilesController : ControllerBase
{
    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "users", "employees", "customers", "items", "categories", "suppliers", "misc"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private const long MaxUploadBytes = 8 * 1024 * 1024; // 8 MB

    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessorService _imageProcessor;
    private readonly Store.DbServices.Abstractions.IVirusScanner? _virusScanner;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        IImageProcessorService imageProcessor,
        ILogger<FilesController> logger,
        Store.DbServices.Abstractions.IVirusScanner? virusScanner = null)
    {
        _fileStorageService = fileStorageService;
        _imageProcessor = imageProcessor;
        _logger = logger;
        _virusScanner = virusScanner;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromQuery] string folder = "misc",
        [FromQuery] int? cropX = null,
        [FromQuery] int? cropY = null,
        [FromQuery] int? cropW = null,
        [FromQuery] int? cropH = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file uploaded."));

        if (file.Length > MaxUploadBytes)
            return BadRequest(ApiResponse.Fail("File exceeds the maximum allowed size of 8 MB."));

        var safeFolder = (folder ?? "misc").Trim().Trim('/', '\\');
        if (string.IsNullOrWhiteSpace(safeFolder) ||
            safeFolder.Contains("..", StringComparison.Ordinal) ||
            safeFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
            !AllowedFolders.Contains(safeFolder.Split('/', '\\')[0]))
        {
            return BadRequest(ApiResponse.Fail("Invalid upload folder."));
        }

        var contentType = file.ContentType?.Split(';')[0].Trim() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType))
            return BadRequest(ApiResponse.Fail("Only JPEG, PNG, WebP, and GIF images are allowed."));

        try
        {
            SixLabors.ImageSharp.Rectangle? cropArea = null;
            if (cropX is >= 0 && cropY is >= 0 && cropW is > 0 && cropH is > 0)
                cropArea = new SixLabors.ImageSharp.Rectangle(cropX.Value, cropY.Value, cropW.Value, cropH.Value);

            await using var rawStream = file.OpenReadStream();

            // ─── Antivirus scan BEFORE processing and saving ────────────────────
            if (_virusScanner is not null)
            {
                await using var scanBuffer = new MemoryStream();
                await rawStream.CopyToAsync(scanBuffer, System.Threading.CancellationToken.None);
                scanBuffer.Position = 0;
                var scan = await _virusScanner.ScanAsync(scanBuffer, file.FileName, default);
                scanBuffer.Position = 0;

                if (!scan.IsClean)
                {
                    _logger.LogWarning("Upload rejected: AV scan flagged {FileName} — {Threat} ({Details})",
                        file.FileName, scan.Threat ?? "n/a", scan.Details ?? "n/a");
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                        ApiResponse.Fail(scan.Threat is null
                            ? "Upload rejected by antivirus scan."
                            : $"Upload rejected: virus signature '{scan.Threat}'."));
                }

                // Re-use the buffered bytes for downstream processing.
                await using var processingStream = new MemoryStream(scanBuffer.ToArray());
                var (thumbStream, fullStream) = await _imageProcessor.ProcessImageAsync(processingStream, cropArea);

                var originalBase = Path.GetFileNameWithoutExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(originalBase))
                    originalBase = "image";

                originalBase = string.Concat(originalBase.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_')).Trim();
                if (string.IsNullOrEmpty(originalBase))
                    originalBase = "image";

                var thumbPath = await _fileStorageService.SaveStreamAsync(thumbStream, originalBase + ".webp", safeFolder + "/thumb");
                var fullPath = await _fileStorageService.SaveStreamAsync(fullStream, originalBase + ".webp", safeFolder + "/full");

                var avResult = new FileUploadResultDto
                {
                    ThumbnailUrl = $"/files/{thumbPath}",
                    FullImageUrl = $"/files/{fullPath}"
                };

                return Ok(ApiResponse<FileUploadResultDto>.Ok(avResult, "File uploaded."));
            }

            // Fallback (no AV configured): continue with the legacy code path.
            var (legacyThumb, legacyFull) = await _imageProcessor.ProcessImageAsync(rawStream, cropArea);

            var originalBase2 = Path.GetFileNameWithoutExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(originalBase2))
                originalBase2 = "image";

            originalBase2 = string.Concat(originalBase2.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_')).Trim();
            if (string.IsNullOrEmpty(originalBase2))
                originalBase2 = "image";

            var legacyThumbPath = await _fileStorageService.SaveStreamAsync(legacyThumb, originalBase2 + ".webp", safeFolder + "/thumb");
            var legacyFullPath = await _fileStorageService.SaveStreamAsync(legacyFull, originalBase2 + ".webp", safeFolder + "/full");

            var legacyResult = new FileUploadResultDto
            {
                ThumbnailUrl = $"/files/{legacyThumbPath}",
                FullImageUrl = $"/files/{legacyFullPath}"
            };

            return Ok(ApiResponse<FileUploadResultDto>.Ok(legacyResult, "File uploaded."));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An error occurred while uploading the file."));
        }
    }

    [HttpDelete]
    public IActionResult DeleteFile([FromQuery] string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return BadRequest(ApiResponse.Fail("Relative path is required."));

        var path = relativePath.Trim();
        if (path.StartsWith("/files/", StringComparison.OrdinalIgnoreCase))
            path = path["/files/".Length..];

        path = path.Replace('\\', '/').TrimStart('/');

        if (path.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(path))
            return BadRequest(ApiResponse.Fail("Invalid file path."));

        var rootSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (rootSegment is null || !AllowedFolders.Contains(rootSegment))
            return BadRequest(ApiResponse.Fail("Invalid file path."));

        try
        {
            _fileStorageService.DeleteFile(path);
            return Ok(ApiResponse.Ok("File deleted."));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An error occurred while deleting the file."));
        }
    }
}
