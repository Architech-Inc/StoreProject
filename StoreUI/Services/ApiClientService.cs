using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Store.Models.DTOs.Common;

namespace StoreUI.Services;

/// <summary>
/// HTTP client for calling the Store API with JWT token support
/// </summary>
public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClientService> _logger;
    private string? _token;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiClientService(HttpClient httpClient, ILogger<ApiClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void SetToken(string? token)
    {
        _token = token;
        if (token is not null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GET {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? data, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling PUT {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DELETE {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling DELETE {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<HttpResponseMessage> GetRawAsync(string endpoint, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync(endpoint, ct);
    }

    private static T? DeserializeResponse<T>(string content)
    {
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
        if (apiResponse is not null)
        {
            if (apiResponse.Success)
            {
                return apiResponse.Data;
            }

            return default;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }
}
