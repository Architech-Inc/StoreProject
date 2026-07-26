using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.API.Infrastructure.Storage;
using System;
using System.Threading.Tasks;

namespace Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly Store.API.Infrastructure.Processing.IImageProcessorService _imageProcessor;

        public FilesController(IFileStorageService fileStorageService, Store.API.Infrastructure.Processing.IImageProcessorService imageProcessor)
        {
            _fileStorageService = fileStorageService;
            _imageProcessor = imageProcessor;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder = "Misc")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var (thumbStream, fullStream) = await _imageProcessor.ProcessImageAsync(stream);

                var thumbPath = await _fileStorageService.SaveStreamAsync(thumbStream, file.FileName + ".webp", folder + "/thumb");
                var fullPath = await _fileStorageService.SaveStreamAsync(fullStream, file.FileName + ".webp", folder + "/full");
                
                return Ok(Store.Models.DTOs.Common.ApiResponse<object>.Ok(new 
                { 
                    success = true, 
                    thumbnailUrl = $"/files/{thumbPath}",
                    fullImageUrl = $"/files/{fullPath}"
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while uploading the file.", details = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult DeleteFile([FromQuery] string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return BadRequest(new { success = false, message = "Relative path is required." });
            }

            // Remove the /files/ prefix if it was passed in
            if (relativePath.StartsWith("/files/"))
            {
                relativePath = relativePath.Substring("/files/".Length);
            }

            try
            {
                _fileStorageService.DeleteFile(relativePath);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the file.", details = ex.Message });
            }
        }
    }
}
