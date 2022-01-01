namespace AzerothMemories.Database.Seeder.Base;

internal static class SetExtensions
{
    private static readonly Func<Name, string, Name>[] _setters = new Func<Name, string, Name>[15];
    private static readonly Dictionary<string, Func<Name, string, Name>> _settersByStr = new();

    static SetExtensions()
    {
        Add(BlizzardLocale.en_US, (x, v) => x with { EnUS = v });
        Add(BlizzardLocale.es_MX, (x, v) => x with { EsMX = v });
        Add(BlizzardLocale.pt_BR, (x, v) => x with { PtBR = v });

        Add(BlizzardLocale.en_GB, (x, v) => x with { EnGB = v });
        Add(BlizzardLocale.es_ES, (x, v) => x with { EsES = v });
        Add(BlizzardLocale.fr_FR, (x, v) => x with { FrFR = v });
        Add(BlizzardLocale.ru_RU, (x, v) => x with { RuRU = v });
        Add(BlizzardLocale.de_DE, (x, v) => x with { DeDE = v });
        //Add(BlizzardLocale.pt_PT, (x, v) => x with { PtPt = v });
        Add(BlizzardLocale.it_IT, (x, v) => x with { ItIT = v });

        Add(BlizzardLocale.ko_KR, (x, v) => x with { KoKR = v });
        Add(BlizzardLocale.zh_TW, (x, v) => x with { ZhTW = v });
        Add(BlizzardLocale.zh_CN, (x, v) => x with { ZhCN = v });
    }

    private static void Add(BlizzardLocale locale, Func<Name, string, Name> func)
    {
        if (_setters[(int)locale] != null)
        {
            throw new NotImplementedException();
        }

        _setters[(int)locale] = func;
        _settersByStr.Add(locale.ToString().Replace("_", string.Empty), func);
    }

    public static Name SetValue(this Name name, BlizzardLocale locale, string field)
    {
        var func = _setters[(int)locale];
        if (func == null)
        {
            return name;
        }

        return func(name, field);
    }

    public static Name SetValue(this Name name, string locale, string field)
    {
        if (_settersByStr.TryGetValue(locale, out var func))
        {
            return func(name, field);
        }

        return name;
    }
}