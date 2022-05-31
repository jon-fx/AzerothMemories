namespace AzerothMemories.Database.Seeder.Base;

internal sealed class WowToolsData
{
    public static readonly string[] AllLocales = { "enUS", "koKR", "frFR", "deDE", "zhCN", "esES", "zhTW", "enGB", "esMX", "ruRU", "ptBR", "itIT", "ptPT" };

    public readonly int Id;
    private readonly Dictionary<string, object> _data;

    public WowToolsData(int id)
    {
        Id = id;

        _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    public void TryAdd(string header, string locale, string fieldValue)
    {
        if (header.EndsWith("_lang"))
        {
            header = header.Replace("_lang", $"_{locale}");
        }

        if (int.TryParse(fieldValue, out var fieldValueInt))
        {
            _data.TryAdd(header, fieldValueInt);
        }
        else
        {
            _data.TryAdd(header, fieldValue);
        }
    }

    public bool TryGetData<TValue>(string fieldHeader, out TValue valueOut)
    {
        if (_data.TryGetValue(fieldHeader, out var valueObject) && valueObject is TValue value)
        {
            valueOut = value;
            return true;
        }

        valueOut = default;

        return false;
    }

    public bool HasLocalised(string name)
    {
        return _data.Keys.Any(x => x.StartsWith(name.Replace("_lang", "_")));
    }

    public BlizzardDataRecordLocal GetLocalised(string name)
    {
        var result = new BlizzardDataRecordLocal();

        if (!name.EndsWith("_lang"))
        {
            throw new NotImplementedException();
        }

        var keys = _data.Keys.Where(x => x.StartsWith(name.Replace("_lang", "_")));
        foreach (var header in keys)
        {
            if (header.Contains("Name_male_") || header.Contains("Name_female_") || header.Contains("Name_lowercase_"))
            {
                continue;
            }

            if (!TryGetData(header, out string value))
            {
                continue;
            }

            SetExtensions.SetValue(result, header, value);
        }

        return result;
    }
}