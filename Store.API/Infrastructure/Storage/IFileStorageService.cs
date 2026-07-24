using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Store.API.Infrastructure.Storage
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subfolder);
        void DeleteFile(string relativePath);
    }
}
