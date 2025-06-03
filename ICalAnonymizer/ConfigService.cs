using System.Security.Authentication;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ICalAnonymizer;

internal class ConfigService
{
    private readonly AppOptions _options;

    public ConfigService(IOptions<AppOptions> options)
    {
        _options = options.Value;
    }
    
    private bool IsLoggedIn(string token) => _options.ApiKey == token;

    public async Task<IDictionary<string, Uri>?> GetCalendars(string key, CancellationToken ct)
    {
        var filePath = $"{_options.ConfigPath}/{key}.json";
        if (!File.Exists(filePath)) return null;
        return await JsonSerializer.DeserializeAsync<IDictionary<string, Uri>>(File.OpenRead(filePath),cancellationToken: ct);
    }
    
    public async Task<CalendarConfig?> GetConfig(string key, string apiKey, CancellationToken ct)
    {
        if (!IsLoggedIn(apiKey)) throw new AuthenticationException("Invalid API Key");
        var filePath = $"{_options.ConfigPath}/{key}.json";
        if (!File.Exists(filePath)) return null;
        return await JsonSerializer.DeserializeAsync<CalendarConfig>(File.OpenRead(filePath),cancellationToken: ct);
    }
    
    public async Task UpdateConfig(string key, string apiKey, CalendarConfig calendars, CancellationToken ct)
    {
        if (!IsLoggedIn(apiKey)) throw new AuthenticationException("Invalid API key");
        var filePath = $"{_options.ConfigPath}/{key}.json";
        await using var fileStream = File.OpenWrite(filePath);
        await JsonSerializer.SerializeAsync(fileStream, calendars, cancellationToken: ct);
    }

    public async Task DeleteConfig(string key, string apiKey, CancellationToken ct)
    {
        if (!IsLoggedIn(apiKey)) throw new AuthenticationException("Invalid API key");
        var filePath = $"{_options.ConfigPath}/{key}.json";
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    public async Task<IList<string>> GetConfigIds(string apiKey, CancellationToken ct)
    {
        if(!IsLoggedIn(apiKey)) throw new AuthenticationException("Invalid API key");
        return Directory.GetFiles(_options.ConfigPath, "*.json").Select(file => Path.GetFileNameWithoutExtension(file)).ToList();
    }
}