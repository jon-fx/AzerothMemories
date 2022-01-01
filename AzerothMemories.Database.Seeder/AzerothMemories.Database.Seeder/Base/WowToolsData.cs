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

    public Name GetLocalised(string name)
    {
        var result = new Name();

        if (!name.EndsWith("_lang"))
        {
            throw new NotImplementedException();
        }

        foreach (var locale in AllLocales)
        {
            var key = name.Replace("_lang", $"_{locale}");

            if (TryGetData<string>(key, out var value))
            {
                result = result.SetValue(locale, value);
            }
        }

        return result;
    }
}