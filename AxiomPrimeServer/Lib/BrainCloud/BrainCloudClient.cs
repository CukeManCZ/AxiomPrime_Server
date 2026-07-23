using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

public class BrainCloudClient
{
    private readonly HttpClient _http;
    private readonly BrainCloudOptions _cfg;

    public BrainCloudClient(HttpClient http, IOptions<BrainCloudOptions> cfg)
    {
        _http = http;
        _cfg = cfg.Value;
    }

    public async Task<string> CallAsync(string service, string operation, object data)
    {
        var requestBody = new
        {
            appId = _cfg.AppId,
            serverName = _cfg.ServerName,
            serverSecret = _cfg.ServerSecret,

            service,
            operation,
            data
        };

        var json = JsonSerializer.Serialize(requestBody);

        var response = await _http.PostAsync(
            "https://api.braincloudservers.com/s2sdispatcher",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        return await response.Content.ReadAsStringAsync();
    }

    // OPTIONAL: strongly typed helper
    public async Task<T?> CallAsync<T>(string service, string operation, object data)
    {
        var raw = await CallAsync(service, operation, data);

        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        JsonElement target;

        // CASE 1: normal { data: {...} }
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("data", out var dataElement))
        {
            target = dataElement;
        }
        // CASE 2: response is JSON string containing JSON (your brainCloud bug/format)
        else if (root.ValueKind == JsonValueKind.String)
        {
            var innerJson = root.GetString();

            if (string.IsNullOrWhiteSpace(innerJson))
                return default;

            using var innerDoc = JsonDocument.Parse(innerJson);
            target = innerDoc.RootElement;
        }
        // CASE 3: already pure object
        else
        {
            target = root;
        }

        return JsonSerializer.Deserialize<T>(
            target.GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
}