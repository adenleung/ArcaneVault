using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ArcaneVault.Api.Services;

public sealed record AiIdentification(
    string ItemType,
    string PossibleName,
    string Brand,
    string Series,
    string ReferenceNumber,
    string ReleaseYear,
    string Description,
    string[] VisibleText,
    double Confidence);

public class OpenAiService(HttpClient http, ILogger<OpenAiService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<AiIdentification> IdentifyAsync(IFormFile image, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new InvalidOperationException("AI Smart Add is not configured.");
        await using var stream = image.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var dataUrl = $"data:{image.ContentType};base64,{Convert.ToBase64String(memory.ToArray())}";
        var schema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new
            {
                itemType = new { type = "string" }, possibleName = new { type = "string" },
                brand = new { type = "string" }, series = new { type = "string" },
                referenceNumber = new { type = "string" }, releaseYear = new { type = "string" },
                description = new { type = "string" }, visibleText = new { type = "array", items = new { type = "string" } },
                confidence = new { type = "number", minimum = 0, maximum = 1 }
            },
            required = new[] { "itemType", "possibleName", "brand", "series", "referenceNumber", "releaseYear", "description", "visibleText", "confidence" }
        };
        var body = new
        {
            model = "gpt-4.1-mini",
            input = new[] { new { role = "user", content = new object[] {
                new { type = "input_text", text = "Identify this collectible using only visible evidence. Do not authenticate it or invent a value. Return concise catalogue search fields." },
                new { type = "input_image", image_url = dataUrl, detail = "high" }
            }}},
            text = new { format = new { type = "json_schema", name = "collectible_identification", strict = true, schema } }
        };
        var json = await SendAsync(body, cancellationToken);
        return JsonSerializer.Deserialize<AiIdentification>(json, JsonOptions)
            ?? throw new InvalidOperationException("The image could not be identified.");
    }

    public async Task<string> AnswerAsync(string question, string collectionContext, CancellationToken cancellationToken)
    {
        if (!IsConfigured) return "AI assistance is temporarily unavailable. You can still use My Collection and Smart Add normally.";
        var body = new
        {
            model = "gpt-4.1-mini",
            input = $$"""
                You are Vault Assistant inside a collectibles-management application. Answer only from the supplied user's collection context and the application guide. Never give investment advice, claim authenticity, invent prices, or discuss another user's records. Keep the answer under 90 words. If data is absent, say so.

                Application guide: Users can add items with Smart Add, edit from My Collection, and view item details. Acquisition value is recorded purchase cost. Estimated value is user-entered and is not a live market price.

                User collection context:
                {{collectionContext}}

                Question: {{question}}
                """
        };
        return await SendAsync(body, cancellationToken);
    }

    private async Task<string> SendAsync(object body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("OpenAI request failed with status {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("The AI service could not complete the request.");
        }
        using var document = JsonDocument.Parse(raw);
        foreach (var output in document.RootElement.GetProperty("output").EnumerateArray())
            if (output.TryGetProperty("content", out var content))
                foreach (var part in content.EnumerateArray())
                    if (part.TryGetProperty("type", out var type) && type.GetString() == "output_text"
                        && part.TryGetProperty("text", out var text)) return text.GetString() ?? string.Empty;
        throw new InvalidOperationException("The AI service returned no usable answer.");
    }
}
