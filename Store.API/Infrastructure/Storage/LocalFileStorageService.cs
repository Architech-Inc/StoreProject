using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Store.API.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
        {
            _logger = logger;
            _basePath = configuration["FileStorage:BasePath"] ?? "./Uploads";

            if (!Path.IsPathRooted(_basePath))
            {
                _basePath = Path.Combine(Directory.GetCurrentDirectory(), _basePath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.", nameof(file));
            }

            var directoryPath = ResolveSafeDirectoryPath(subfolder);
            if (directoryPath is null)
            {
                throw new ArgumentException("Invalid or unsafe subfolder path.", nameof(subfolder));
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || extension.Contains("..", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".webp";
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(directoryPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for URL access (e.g. "Images/Products/abc.jpg")
            return Path.Combine(subfolder, uniqueFileName).Replace("\\", "/");
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fullPath = ResolveSafePath(relativePath);
            if (fullPath is null) return;

            if (File.Exists(fullPath))
            {
                try
                {
                    // SEC-12: Reject deletion of symbolic links and reparse points
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) || !string.IsNullOrEmpty(fileInfo.LinkTarget))
                    {
                        _logger.LogWarning("Rejected deletion of symbolic link / reparse point at {Path}", fullPath);
                        return;
                    }

                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete file at {Path}", fullPath);
                }
            }
        }

        private string? ResolveSafePath(string relativePath)
        {
            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            if (normalized.Contains("..", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains('~') ||
                normalized.Contains(':') ||
                Path.IsPathRooted(normalized) ||
                normalized.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                _logger.LogWarning("Rejected unsafe relative path: {Path}", relativePath);
                return null;
            }

            var combined = Path.GetFullPath(Path.Combine(_basePath, normalized.Replace('/', Path.DirectorySeparatorChar)));
            var root = Path.GetFullPath(_basePath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(combined, Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Rejected path traversal attempt: {Path}", relativePath);
                return null;
            }

            return combined;
        }

        private string? ResolveSafeDirectoryPath(string subfolder)
        {
            var normalized = (subfolder ?? string.Empty).Replace('\\', '/').Trim().TrimStart('/');
            if (normalized.Contains("..", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains('~') ||
                normalized.Contains(':') ||
                Path.IsPathRooted(normalized) ||
                normalized.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                _logger.LogWarning("Rejected unsafe subfolder path: {Subfolder}", subfolder);
                return null;
            }

            var combined = Path.GetFullPath(Path.Combine(_basePath, normalized.Replace('/', Path.DirectorySeparatorChar)));
            var root = Path.GetFullPath(_basePath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(combined, Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Rejected path traversal in subfolder: {Subfolder}", subfolder);
                return null;
            }

            if (Directory.Exists(combined))
            {
                var dirInfo = new DirectoryInfo(combined);
                if (dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) || !string.IsNullOrEmpty(dirInfo.LinkTarget))
                {
                    _logger.LogWarning("Rejected operation targeting symbolic link / reparse point directory: {Path}", combined);
                    return null;
                }
            }

            return combined;
        }

        public async Task<string> SaveStreamAsync(Stream stream, string fileName, string subfolder)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentException("Stream is empty or null.", nameof(stream));
            }

            var directoryPath = ResolveSafeDirectoryPath(subfolder);
            if (directoryPath is null)
            {
                throw new ArgumentException("Invalid or unsafe subfolder path.", nameof(subfolder));
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || extension.Contains("..", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".webp";
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(directoryPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            return Path.Combine(subfolder, uniqueFileName).Replace("\\", "/");
        }
    }
}
