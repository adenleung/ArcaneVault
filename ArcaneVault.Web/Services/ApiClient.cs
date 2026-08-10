/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using ArcaneVault.Web.Models;

namespace ArcaneVault.Web.Services;

public class ApiClient(HttpClient http, IHttpContextAccessor context, ILogger<ApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private HttpRequestMessage Request(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        var user = context.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var token = user.FindFirst("ArcaneVaultApiToken")?.Value;
            if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        using var response = await SendAsync(Request(HttpMethod.Get, url));
        if (response.StatusCode == HttpStatusCode.NotFound) return default;
        await EnsureSuccess(response); return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
    public async Task<T?> PostAsync<T>(string url, object body)
    {
        using var response = await SendAsync(Request(HttpMethod.Post, url, body));
        await EnsureSuccess(response); return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
    public async Task<T?> PostFileAsync<T>(string url, IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        await using var source = file.OpenReadStream();
        using var content = new StreamContent(source);
        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(content, "image", file.FileName);
        using var request = Request(HttpMethod.Post, url);
        request.Content = form;
        using var response = await SendAsync(request);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
    public async Task PutAsync(string url, object body)
    {
        using var response = await SendAsync(Request(HttpMethod.Put, url, body)); await EnsureSuccess(response);
    }
    public async Task DeleteAsync(string url)
    {
        using var response = await SendAsync(Request(HttpMethod.Delete, url)); await EnsureSuccess(response);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        try { return await http.SendAsync(request); }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Arcane Vault API is unavailable at {BaseAddress}.", http.BaseAddress);
            throw new ApiException("The Arcane Vault API is not running. Start both solution projects and try again.", HttpStatusCode.ServiceUnavailable);
        }
    }

    private async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var raw = await response.Content.ReadAsStringAsync();
        logger.LogError("Arcane Vault API returned {StatusCode}. Response: {Response}", (int)response.StatusCode, raw);
        try
        {
            using var document = JsonDocument.Parse(raw);
            var message = document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("message", out var property) ? property.GetString() : null;
            throw new ApiException(message ?? $"The API returned error {(int)response.StatusCode}.", response.StatusCode);
        }
        catch (JsonException) { throw new ApiException($"The API returned error {(int)response.StatusCode}. Check the ArcaneVault.Api Output window.", response.StatusCode); }
    }
}

public class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
