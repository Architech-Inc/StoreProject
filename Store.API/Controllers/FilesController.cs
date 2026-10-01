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

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
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
            safeFolder.Contains("..", StringComparison.OrdinalIgnoreCase) ||
            safeFolder.Contains('~') ||
            safeFolder.Contains(':') ||
            safeFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return BadRequest(ApiResponse.Fail("Invalid upload folder."));
        }

        var folderSegments = safeFolder.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        if (folderSegments.Length == 0 ||
            !AllowedFolders.Contains(folderSegments[0]) ||
            folderSegments.Any(s => s == ".." || s == "." || s.Contains('~') || s.Contains(':')))
        {
            return BadRequest(ApiResponse.Fail("Invalid upload folder."));
        }

        if (string.IsNullOrWhiteSpace(file.FileName) ||
            file.FileName.Contains("..", StringComparison.OrdinalIgnoreCase) ||
            file.FileName.Contains('~') ||
            file.FileName.Contains(':') ||
            file.FileName.IndexOfAny(Path.GetInvalidFileNameChars().Except(new[] { '/', '\\' }).ToArray()) >= 0)
        {
            return BadRequest(ApiResponse.Fail("Invalid file name."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse.Fail("Only JPEG, PNG, WebP, and GIF images are allowed."));
        }

        var contentType = file.ContentType?.Split(';')[0].Trim() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType))
        {
            return BadRequest(ApiResponse.Fail("Only JPEG, PNG, WebP, and GIF images are allowed."));
        }

        try
        {
            SixLabors.ImageSharp.Rectangle? cropArea = null;
            if (cropX is >= 0 && cropY is >= 0 && cropW is > 0 && cropH is > 0)
                cropArea = new SixLabors.ImageSharp.Rectangle(cropX.Value, cropY.Value, cropW.Value, cropH.Value);

            await using var rawStream = file.OpenReadStream();

            // ─── Magic byte inspection (MIME sniffing prevention) ───────────────
            if (!HasValidImageHeader(rawStream))
            {
                return BadRequest(ApiResponse.Fail("The uploaded file content does not match allowed image formats (JPEG, PNG, WebP, GIF)."));
            }

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
        catch (SixLabors.ImageSharp.UnknownImageFormatException)
        {
            return BadRequest(ApiResponse.Fail("The uploaded image file is corrupt or invalid."));
        }
        catch (SixLabors.ImageSharp.InvalidImageContentException)
        {
            return BadRequest(ApiResponse.Fail("The uploaded image file contains invalid image data."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file {FileName}", file.FileName);
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

        if (path.Contains("..", StringComparison.OrdinalIgnoreCase) ||
            path.Contains('~') ||
            path.Contains(':') ||
            Path.IsPathRooted(path) ||
            path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return BadRequest(ApiResponse.Fail("Invalid file path."));
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var rootSegment = segments.FirstOrDefault();
        if (rootSegment is null || !AllowedFolders.Contains(rootSegment) || segments.Any(s => s == ".." || s == "." || s.Contains('~') || s.Contains(':')))
        {
            return BadRequest(ApiResponse.Fail("Invalid file path."));
        }

        try
        {
            _fileStorageService.DeleteFile(path);
            return Ok(ApiResponse.Ok("File deleted."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {Path}", path);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("An error occurred while deleting the file."));
        }
    }

    private static bool HasValidImageHeader(Stream stream)
    {
        if (stream.Length < 12) return false;

        var header = new byte[12];
        var originalPos = stream.CanSeek ? stream.Position : 0;
        if (stream.CanSeek) stream.Position = 0;
        var read = stream.Read(header, 0, 12);
        if (stream.CanSeek) stream.Position = originalPos;
        if (read < 12) return false;

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return true;

        // GIF: GIF87a or GIF89a
        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38 &&
            (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61)
            return true;

        // WebP: RIFF....WEBP
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return true;

        return false;
    }
}
