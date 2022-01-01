namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaResourceWriter
{
    private readonly Dictionary<string, string>[] _resourcesByLocal;

    public MoaResourceWriter()
    {
        _resourcesByLocal = new Dictionary<string, string>[15];
        for (var i = 0; i < _resourcesByLocal.Length; i++)
        {
            _resourcesByLocal[i] = new Dictionary<string, string>();
        }
    }

    public void AddLocalizationData(string key, Name data)
    {
        AddLocalizationData(BlizzardLocale.en_US, key, data.EnUS);
        AddLocalizationData(BlizzardLocale.es_MX, key, data.EsMX);
        AddLocalizationData(BlizzardLocale.pt_BR, key, data.PtBR);

        AddLocalizationData(BlizzardLocale.en_GB, key, data.EnGB);
        AddLocalizationData(BlizzardLocale.es_ES, key, data.EsES);
        AddLocalizationData(BlizzardLocale.fr_FR, key, data.FrFR);
        AddLocalizationData(BlizzardLocale.ru_RU, key, data.RuRU);
        AddLocalizationData(BlizzardLocale.de_DE, key, data.DeDE);
        AddLocalizationData(BlizzardLocale.pt_PT, key, data.PtBR);
        AddLocalizationData(BlizzardLocale.it_IT, key, data.ItIT);

        AddLocalizationData(BlizzardLocale.ko_KR, key, data.KoKR);
        AddLocalizationData(BlizzardLocale.zh_TW, key, data.ZhTW);
        AddLocalizationData(BlizzardLocale.zh_CN, key, data.ZhCN);
    }

    public void AddLocalizationData(string key, Name data, Func<BlizzardLocale, string, string> func)
    {
        AddLocalizationData(BlizzardLocale.en_US, key, data.EnUS, func);
        AddLocalizationData(BlizzardLocale.es_MX, key, data.EsMX, func);
        AddLocalizationData(BlizzardLocale.pt_BR, key, data.PtBR, func);

        AddLocalizationData(BlizzardLocale.en_GB, key, data.EnGB, func);
        AddLocalizationData(BlizzardLocale.es_ES, key, data.EsES, func);
        AddLocalizationData(BlizzardLocale.fr_FR, key, data.FrFR, func);
        AddLocalizationData(BlizzardLocale.ru_RU, key, data.RuRU, func);
        AddLocalizationData(BlizzardLocale.de_DE, key, data.DeDE, func);
        AddLocalizationData(BlizzardLocale.pt_PT, key, data.PtBR, func);
        AddLocalizationData(BlizzardLocale.it_IT, key, data.ItIT, func);

        AddLocalizationData(BlizzardLocale.ko_KR, key, data.KoKR, func);
        AddLocalizationData(BlizzardLocale.zh_TW, key, data.ZhTW, func);
        AddLocalizationData(BlizzardLocale.zh_CN, key, data.ZhCN, func);
    }

    public void AddCommonLocalizationData(string key, string value)
    {
        AddLocalizationData(BlizzardLocale.None, key, value);
    }

    private void AddLocalizationData(BlizzardLocale locale, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var id = (int)locale;
        if (_resourcesByLocal[id].TryGetValue(key, out var oldValue))
        {
            Exceptions.ThrowIf(value != oldValue);
        }

        _resourcesByLocal[id][key] = value;
    }

    private void AddLocalizationData(BlizzardLocale locale, string key, string value, Func<BlizzardLocale, string, string> func)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var id = (int)locale;
        var newValue = func(locale, value);
        if (_resourcesByLocal[id].TryGetValue(key, out var oldValue))
        {
            Exceptions.ThrowIf(newValue != oldValue);
        }

        _resourcesByLocal[id][key] = newValue;
    }

    public void CloneLocalizationData(string oldKey, string newKey)
    {
        foreach (var dictionary in _resourcesByLocal)
        {
            if (dictionary.TryGetValue(oldKey, out var value))
            {
                dictionary[newKey] = value;
            }
        }
    }

    public bool GetLocalizationData(BlizzardLocale locale, string key, out string value)
    {
        var id = (int)locale;
        return _resourcesByLocal[id].TryGetValue(key, out value);
    }

    public void Save()
    {
        //for (var i = 0; i < _resourcesByLocal.Length; i++)
        //{
        //    var resources = _resourcesByLocal[i];
        //    if (resources == null || resources.Count == 0)
        //    {
        //        continue;
        //    }

        //    var stringBuilder = new StringBuilder();
        //    using var stringWriter = new StringWriter(stringBuilder);
        //    using var writer = new JsonTextWriter(stringWriter);
        //    writer.Formatting = Formatting.Indented;
        //    writer.WriteStartObject();

        //    foreach (var resource in resources)
        //    {
        //        writer.WritePropertyName(resource.Key);
        //        writer.WriteValue(resource.Value);
        //    }

        //    writer.WriteEndObject();
        //    writer.Flush();

        //    var outputFile = GetOuputFile(i);
        //    File.WriteAllText(outputFile, stringBuilder.ToString());
        //}
    }

    //private string GetOuputFile(int index)
    //{
    //    var locale = (BlizzardLocale)index;
    //    var filePath = @"..\..\..\AzerothMemories.WebBlazor\AzerothMemories.WebBlazor\Resources\Resources.json";
    //    if (locale != BlizzardLocale.None)
    //    {
    //        filePath = $@"..\..\..\AzerothMemories.WebBlazor\AzerothMemories.WebBlazor\Resources\Resources.{locale.ToString().Replace('_', '-')}.json";
    //    }

    //    if (File.Exists(filePath))
    //    {
    //        File.Delete(filePath);
    //    }

    //    return filePath;
    //}
}