using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Store.API.Infrastructure.Storage
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
        Task<string> SaveStreamAsync(System.IO.Stream stream, string fileName, string subfolder);
        void DeleteFile(string relativePath);
    }
}
