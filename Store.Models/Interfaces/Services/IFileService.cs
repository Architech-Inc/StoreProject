using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Store.Models.Interfaces.Services
{
    public interface IFileService
    {
        Task<(string? ThumbnailUrl, string? FullImageUrl)> UploadFileAsync(
            Stream fileStream, 
            string fileName, 
            string contentType, 
            string folder, 
            int? cropX = null, int? cropY = null, int? cropW = null, int? cropH = null,
            CancellationToken ct = default);
            
        Task<bool> DeleteFileAsync(string relativePath, CancellationToken ct = default);
    }
}
