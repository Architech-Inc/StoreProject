using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.API.Controllers;
using Store.API.Infrastructure.Processing;
using Store.API.Infrastructure.Storage;
using Store.DbServices.Abstractions;
using Store.Models.DTOs.Common;
using System.Text;
using Xunit;

namespace Store.API.Tests;

public class FilesControllerSecurityTests
{
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IImageProcessorService> _imageProcessorMock;
    private readonly Mock<IVirusScanner> _virusScannerMock;
    private readonly FilesController _controller;

    // Standard valid 16-byte PNG header and minimal payload
    private static readonly byte[] ValidPngBytes = new byte[]
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52
    };

    // Standard valid 16-byte JPEG header and minimal payload
    private static readonly byte[] ValidJpegBytes = new byte[]
    {
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
        0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60
    };

    public FilesControllerSecurityTests()
    {
        _fileStorageMock = new Mock<IFileStorageService>();
        _imageProcessorMock = new Mock<IImageProcessorService>();
        _virusScannerMock = new Mock<IVirusScanner>();

        _controller = new FilesController(
            _fileStorageMock.Object,
            _imageProcessorMock.Object,
            NullLogger<FilesController>.Instance,
            _virusScannerMock.Object);
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(content.Length);
        mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(content));
        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((target, _) =>
            {
                target.Write(content, 0, content.Length);
                return Task.CompletedTask;
            });
        return mock.Object;
    }

    [Fact]
    public async Task UploadFile_NullOrEmptyFile_ReturnsBadRequest()
    {
        var resultNull = await _controller.UploadFile(null!);
        var badRequestNull = Assert.IsType<BadRequestObjectResult>(resultNull);
        var apiResponseNull = Assert.IsType<ApiResponse>(badRequestNull.Value);
        Assert.False(apiResponseNull.Success);

        var emptyFile = CreateMockFormFile("test.png", "image/png", Array.Empty<byte>());
        var resultEmpty = await _controller.UploadFile(emptyFile);
        var badRequestEmpty = Assert.IsType<BadRequestObjectResult>(resultEmpty);
        var apiResponseEmpty = Assert.IsType<ApiResponse>(badRequestEmpty.Value);
        Assert.False(apiResponseEmpty.Success);
    }

    [Fact]
    public async Task UploadFile_ExceedsSizeLimit_ReturnsBadRequest()
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.Length).Returns(9 * 1024 * 1024); // 9 MB > 8 MB
        mock.Setup(f => f.FileName).Returns("large.png");

        var result = await _controller.UploadFile(mock.Object);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("maximum allowed size", apiResponse.Message);
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("..\\secret")]
    [InlineData("misc/../../etc")]
    [InlineData("~/uploads")]
    [InlineData("misc/~")]
    [InlineData("misc:stream")]
    [InlineData("unauthorized_folder")]
    [InlineData("admin")]
    public async Task UploadFile_InvalidOrTraversalFolder_ReturnsBadRequest(string maliciousFolder)
    {
        var file = CreateMockFormFile("avatar.png", "image/png", ValidPngBytes);

        var result = await _controller.UploadFile(file, folder: maliciousFolder);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Equal("Invalid upload folder.", apiResponse.Message);
    }

    [Theory]
    [InlineData("../evil.png")]
    [InlineData("..\\evil.png")]
    [InlineData("~evil.png")]
    [InlineData("evil:stream.png")]
    [InlineData("evil\0.png")]
    public async Task UploadFile_PathTraversalFileName_ReturnsBadRequest(string maliciousFileName)
    {
        var file = CreateMockFormFile(maliciousFileName, "image/png", ValidPngBytes);

        var result = await _controller.UploadFile(file, folder: "users");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Equal("Invalid file name.", apiResponse.Message);
    }

    [Theory]
    [InlineData("shell.php", "image/jpeg")]
    [InlineData("script.sh", "image/png")]
    [InlineData("malware.exe", "image/webp")]
    [InlineData("document.pdf", "image/jpeg")]
    [InlineData("vector.svg", "image/svg+xml")]
    public async Task UploadFile_DisallowedExtension_ReturnsBadRequest(string fileName, string contentType)
    {
        var file = CreateMockFormFile(fileName, contentType, ValidPngBytes);

        var result = await _controller.UploadFile(file, folder: "items");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("Only JPEG, PNG, WebP, and GIF images are allowed", apiResponse.Message);
    }

    [Theory]
    [InlineData("application/x-msdownload")]
    [InlineData("text/html")]
    [InlineData("application/javascript")]
    [InlineData("application/octet-stream")]
    public async Task UploadFile_DisallowedContentType_ReturnsBadRequest(string disallowedContentType)
    {
        var file = CreateMockFormFile("valid.png", disallowedContentType, ValidPngBytes);

        var result = await _controller.UploadFile(file, folder: "items");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("Only JPEG, PNG, WebP, and GIF images are allowed", apiResponse.Message);
    }

    [Fact]
    public async Task UploadFile_SpoofedMagicBytes_ReturnsBadRequest()
    {
        // Extension is .jpg and contentType is image/jpeg, but content is plain ASCII/executable text
        var fakeJpgBytes = Encoding.UTF8.GetBytes("MZ_THIS_IS_NOT_A_JPEG_FILE_PAYLOAD");
        var file = CreateMockFormFile("photo.jpg", "image/jpeg", fakeJpgBytes);

        var result = await _controller.UploadFile(file, folder: "items");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Contains("does not match allowed image formats", apiResponse.Message);
    }

    [Fact]
    public async Task UploadFile_AntivirusFlagged_Returns415UnsupportedMediaType()
    {
        var file = CreateMockFormFile("infected.png", "image/png", ValidPngBytes);

        _virusScannerMock
            .Setup(s => s.ScanAsync(It.IsAny<Stream>(), "infected.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScanResult.Infected("Eicar-Test-Signature", "ClamAV test virus"));

        var result = await _controller.UploadFile(file, folder: "users");

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, statusResult.StatusCode);
        var apiResponse = Assert.IsType<ApiResponse>(statusResult.Value);
        Assert.Contains("virus signature 'Eicar-Test-Signature'", apiResponse.Message);
    }

    [Fact]
    public async Task UploadFile_ValidCleanImage_ProcessesAndReturnsUrls()
    {
        var file = CreateMockFormFile("valid_avatar.jpg", "image/jpeg", ValidJpegBytes);

        _virusScannerMock
            .Setup(s => s.ScanAsync(It.IsAny<Stream>(), "valid_avatar.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ScanResult.Clean());

        var thumbStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var fullStream = new MemoryStream(new byte[] { 4, 5, 6 });

        _imageProcessorMock
            .Setup(p => p.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<SixLabors.ImageSharp.Rectangle?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((thumbStream, fullStream));

        _fileStorageMock
            .Setup(s => s.SaveStreamAsync(thumbStream, "valid_avatar.webp", "users/thumb"))
            .ReturnsAsync("users/thumb/uuid1.webp");

        _fileStorageMock
            .Setup(s => s.SaveStreamAsync(fullStream, "valid_avatar.webp", "users/full"))
            .ReturnsAsync("users/full/uuid2.webp");

        var result = await _controller.UploadFile(file, folder: "users");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<FileUploadResultDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("/files/users/thumb/uuid1.webp", apiResponse.Data!.ThumbnailUrl);
        Assert.Equal("/files/users/full/uuid2.webp", apiResponse.Data!.FullImageUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeleteFile_EmptyPath_ReturnsBadRequest(string emptyPath)
    {
        var result = _controller.DeleteFile(emptyPath);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Equal("Relative path is required.", apiResponse.Message);
    }

    [Theory]
    [InlineData("../secret.webp")]
    [InlineData("..\\secret.webp")]
    [InlineData("/files/users/../../etc/passwd")]
    [InlineData("/files/users/~/evil.png")]
    [InlineData("/files/users/stream:colon")]
    [InlineData("/etc/passwd")]
    [InlineData("C:/Windows/system.ini")]
    [InlineData("/files/unauthorized_folder/file.webp")]
    public void DeleteFile_PathTraversalOrInvalidFolder_ReturnsBadRequest(string maliciousPath)
    {
        var result = _controller.DeleteFile(maliciousPath);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequest.Value);
        Assert.Equal("Invalid file path.", apiResponse.Message);
    }

    [Fact]
    public void DeleteFile_ValidPath_CallsStorageDeleteAndReturnsOk()
    {
        var validPath = "/files/items/full/12345678-1234-1234-1234-123456789abc.webp";

        var result = _controller.DeleteFile(validPath);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("File deleted.", apiResponse.Message);

        _fileStorageMock.Verify(s => s.DeleteFile("items/full/12345678-1234-1234-1234-123456789abc.webp"), Times.Once);
    }

    [Fact]
    public void LocalFileStorageService_ResolveSafePath_PreventsTraversal()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "StoreProject_Storage_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["FileStorage:BasePath"]).Returns(tempDir);

            var storage = new LocalFileStorageService(configMock.Object, NullLogger<LocalFileStorageService>.Instance);

            // Attempt deletion outside base path via traversal
            storage.DeleteFile("../../secret_outside.txt");
            storage.DeleteFile("items/~/malicious.txt");
            storage.DeleteFile("items/foo:bar");

            // Verify a real file can be deleted safely
            var testSubdir = Path.Combine(tempDir, "items");
            Directory.CreateDirectory(testSubdir);
            var testFile = Path.Combine(testSubdir, "test.webp");
            File.WriteAllText(testFile, "hello");

            Assert.True(File.Exists(testFile));
            storage.DeleteFile("items/test.webp");
            Assert.False(File.Exists(testFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
