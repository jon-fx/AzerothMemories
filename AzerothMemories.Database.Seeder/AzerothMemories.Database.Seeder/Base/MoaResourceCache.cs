using System.Runtime.CompilerServices;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceCache
{
    private readonly WowTools _wowTools;
    private readonly string _resourceFilePath;

    private readonly string _seperator = "|>*<|";
    private Dictionary<string, string> _allResources;

    public MoaResourceCache(WowTools wowTools)
    {
        _wowTools = wowTools;
        _allResources = new Dictionary<string, string>();
        _resourceFilePath = Path.Combine(@"C:\Users\John\Desktop\Stuff\BlizzardData\", "BlizzardResources");
    }

    public bool RequestData { get; set; } = true;

    public WowTools WowTools => _wowTools;

    public async Task<T> GetOrRequestData<T>(string key, Func<string, Task<RequestResult<T>>> callback, [CallerArgumentExpression("callback")] string callbackExpression = default) where T : class
    {
        T jsonData = null;

        key += $"-||-{callbackExpression}";

        if (_allResources.TryGetValue(key, out var value))
        {
            if (string.IsNullOrWhiteSpace(value))
            {
            }
            else
            {
                jsonData = JsonSerializer.Deserialize<T>(value);
            }
        }
        else if (RequestData)
        {
            var result = await callback(key);
            if (string.IsNullOrWhiteSpace(result.ResultString))
            {
            }
            else
            {
                jsonData = result.ResultData;
            }

            _allResources[key] = result.ResultString;

            var line = $"{key}{_seperator}{result.ResultString}";
            await File.AppendAllLinesAsync(_resourceFilePath, new[] { line });
        }

        return jsonData;
    }

    public async Task TryLoadResources()
    {
        if (_allResources.Count == 0 && File.Exists(_resourceFilePath))
        {
            var lines = await File.ReadAllLinesAsync(_resourceFilePath);
            var splitLines = lines.Select(x => x.Split(_seperator));
            _allResources = splitLines.ToDictionary(x => x[0], x => x[1], StringComparer.OrdinalIgnoreCase);
        }
    }
}