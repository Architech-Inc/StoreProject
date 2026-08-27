using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiCategoryService : ICategoryService
{
    private readonly IApiClientService _client;

    public ApiCategoryService(IApiClientService client) => _client = client;

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _client.GetAsync<IEnumerable<Category>>("/api/categories");
        return result ?? Enumerable.Empty<Category>();
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _client.GetAsync<Category>($"/api/categories/{id}");

    public async Task<Category> CreateAsync(string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default)
    {
        var req = new { Name = name, Description = description, ThumbnailUrl = thumbnailUrl, FullImageUrl = fullImageUrl };
        var result = await _client.PostAsync<Category>("/api/categories", req);
        return result ?? throw new InvalidOperationException("Failed to create category.");
    }

    public async Task<Category?> UpdateAsync(int id, string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default)
    {
        var req = new { Name = name, Description = description, ThumbnailUrl = thumbnailUrl, FullImageUrl = fullImageUrl };
        return await _client.PutAsync<Category>($"/api/categories/{id}", req);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        => await _client.DeleteAsync($"/api/categories/{id}");
}
