namespace AzerothMemories.WebBlazor.Common;

public static class ServerSideLocaleExt
{
    private static readonly string[] _wowheadLocales;
    private static readonly Dictionary<CultureInfo, ServerSideLocale> _namesToLocales;

    static ServerSideLocaleExt()
    {

        _wowheadLocales = new string[(int)ServerSideLocale.Count];
        _wowheadLocales[(int)ServerSideLocale.None] = null;
        _wowheadLocales[(int)ServerSideLocale.En_Us] = null;
        _wowheadLocales[(int)ServerSideLocale.Es_Mx] = "es";
        _wowheadLocales[(int)ServerSideLocale.Pt_Br] = "pt";

        _wowheadLocales[(int)ServerSideLocale.En_Gb] = null;
        _wowheadLocales[(int)ServerSideLocale.Es_Es] = "es";
        _wowheadLocales[(int)ServerSideLocale.Fr_Fr] = "fr";
        _wowheadLocales[(int)ServerSideLocale.Ru_Ru] = "ru";
        _wowheadLocales[(int)ServerSideLocale.De_De] = "de";
        _wowheadLocales[(int)ServerSideLocale.Pt_Pt] = "pt";
        _wowheadLocales[(int)ServerSideLocale.It_It] = "it";

        _wowheadLocales[(int)ServerSideLocale.Ko_Kr] = "ko";
        _wowheadLocales[(int)ServerSideLocale.Zh_Tw] = null;
        _wowheadLocales[(int)ServerSideLocale.Zh_Cn] = "cn";

        var allEnums = Enum.GetValues<ServerSideLocale>().Where(x => x > ServerSideLocale.None && x < ServerSideLocale.Count);
        _namesToLocales = allEnums.Select(x => new { key = x.ToString().Replace('_', '-'), Value = x }).ToDictionary(x => CultureInfo.GetCultureInfo(x.key), x => x.Value);
    }

    public static ServerSideLocale GetServerSideLocale()
    {
        var cultureInfo = CultureInfo.CurrentCulture;
        if (_namesToLocales.TryGetValue(cultureInfo, out var serverSideLocale))
        {
            return serverSideLocale;
        }

        return ServerSideLocale.None;
    }

    public static string GetWowHeadDomain()
    {
        var locale = GetServerSideLocale();
        var wowhead = _wowheadLocales[(int) locale];
        if (wowhead == null)
        {
            return "";
        }
        
        return $"&domain={wowhead}";
    }
}