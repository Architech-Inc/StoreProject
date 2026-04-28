using System.Net.Http.Headers;

namespace StoreUI.Services;

/// <summary>
/// HTTP client wrapper for calling the Store API with JWT token management
/// </summary>
public interface IApiClientService
{
    /// <summary>Set the JWT access token for authenticated requests</summary>
    void SetToken(string? token);

    /// <summary>GET request to API</summary>
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default);

    /// <summary>POST request to API</summary>
    Task<T?> PostAsync<T>(string endpoint, object? data, CancellationToken ct = default);

    /// <summary>PUT request to API</summary>
    Task<T?> PutAsync<T>(string endpoint, object? data, CancellationToken ct = default);

    /// <summary>DELETE request to API</summary>
    Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default);

    /// <summary>Raw HTTP GET for non-JSON responses</summary>
    Task<HttpResponseMessage> GetRawAsync(string endpoint, CancellationToken ct = default);
}
