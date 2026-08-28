using Microsoft.AspNetCore.Http;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class LookupManager : ILookupManager
{
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;

    public LookupManager(IApiClientService apiClient, IFileService fileService)
    {
        _apiClient = apiClient;
        _fileService = fileService;
    }

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return (await _apiClient.GetAsync<List<Category>>("/api/categories", ct)) ?? new();
    }

    public async Task<List<Unit>> GetUnitsAsync(CancellationToken ct = default)
    {
        return (await _apiClient.GetAsync<List<Unit>>("/api/units", ct)) ?? new();
    }

    public async Task<List<Department>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        return (await _apiClient.GetAsync<List<Department>>("/api/departments", ct)) ?? new();
    }

    public async Task SaveCategoryAsync(int id, string name, string? description, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default)
    {
        string? thumbUrl = null;
        string? fullUrl = null;
        if (image != null && image.Length > 0)
        {
            if (id != 0)
            {
                var existingCategory = await _apiClient.GetAsync<Category>($"/api/categories/{id}", ct);
                if (existingCategory != null)
                {
                    if (!string.IsNullOrWhiteSpace(existingCategory.ThumbnailUrl))
                        await _fileService.DeleteFileAsync(existingCategory.ThumbnailUrl, ct);
                    if (!string.IsNullOrWhiteSpace(existingCategory.FullImageUrl))
                        await _fileService.DeleteFileAsync(existingCategory.FullImageUrl, ct);
                }
            }
            using var stream = image.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, image.FileName, image.ContentType, "categories", cropX, cropY, cropW, cropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        if (id == 0)
            await _apiClient.PostAsync<Category>("/api/categories", new { name, description, thumbnailUrl = thumbUrl, fullImageUrl = fullUrl }, ct);
        else
            await _apiClient.PutAsync<Category>($"/api/categories/{id}", new { name, description, thumbnailUrl = thumbUrl, fullImageUrl = fullUrl }, ct);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        return await _apiClient.DeleteAsync($"/api/categories/{id}", ct);
    }

    public async Task SaveUnitAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        if (id == 0)
            await _apiClient.PostAsync<Unit>("/api/units", new { name, abbreviation, description }, ct);
        else
            await _apiClient.PutAsync<Unit>($"/api/units/{id}", new { name, abbreviation, description }, ct);
    }

    public async Task<bool> DeleteUnitAsync(int id, CancellationToken ct = default)
    {
        return await _apiClient.DeleteAsync($"/api/units/{id}", ct);
    }

    public async Task SaveDepartmentAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        if (id == 0)
            await _apiClient.PostAsync<Department>("/api/departments", new { name, description }, ct);
        else
            await _apiClient.PutAsync<Department>($"/api/departments/{id}", new { name, description }, ct);
    }

    public async Task<bool> DeleteDepartmentAsync(int id, CancellationToken ct = default)
    {
        return await _apiClient.DeleteAsync($"/api/departments/{id}", ct);
    }
}
