namespace AzerothMemories.WebServer.Services;

public static class ServerLocaleHelpers
{
    private static readonly Func<BlizzardDataRecordLocal, string>[] _getterFunc;
    private static readonly Func<AppDbContext, string, IQueryable<BlizzardDataRecord>>[] _searchFunc;

    static ServerLocaleHelpers()
    {
        _getterFunc = new Func<BlizzardDataRecordLocal, string>[(int)ServerSideLocale.Count];
        _getterFunc[(int)ServerSideLocale.None] = x => x.EnUs;
        _getterFunc[(int)ServerSideLocale.En_Us] = x => x.EnUs;
        _getterFunc[(int)ServerSideLocale.Es_Mx] = x => x.EsMx;
        _getterFunc[(int)ServerSideLocale.Pt_Br] = x => x.PtBr;

        _getterFunc[(int)ServerSideLocale.En_Gb] = x => x.EnGb;
        _getterFunc[(int)ServerSideLocale.Es_Es] = x => x.EsEs;
        _getterFunc[(int)ServerSideLocale.Fr_Fr] = x => x.FrFr;
        _getterFunc[(int)ServerSideLocale.Ru_Ru] = x => x.RuRu;
        _getterFunc[(int)ServerSideLocale.De_De] = x => x.DeDe;
        _getterFunc[(int)ServerSideLocale.Pt_Pt] = x => x.PtPt;
        _getterFunc[(int)ServerSideLocale.It_It] = x => x.ItIt;

        _getterFunc[(int)ServerSideLocale.Ko_Kr] = x => x.KoKr;
        _getterFunc[(int)ServerSideLocale.Zh_Tw] = x => x.ZhTw;
        _getterFunc[(int)ServerSideLocale.Zh_Cn] = x => x.ZhCn;

        _searchFunc = new Func<AppDbContext, string, IQueryable<BlizzardDataRecord>>[(int)ServerSideLocale.Count];
        _searchFunc[(int)ServerSideLocale.None] = (database, searchString) => database.BlizzardData.Where(r => r.Name.EnUs.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.En_Us] = (database, searchString) => database.BlizzardData.Where(r => r.Name.EnUs.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Es_Mx] = (database, searchString) => database.BlizzardData.Where(r => r.Name.EsMx.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Pt_Br] = (database, searchString) => database.BlizzardData.Where(r => r.Name.PtBr.ToLower().StartsWith(searchString));

        _searchFunc[(int)ServerSideLocale.En_Gb] = (database, searchString) => database.BlizzardData.Where(r => r.Name.EnGb.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Es_Es] = (database, searchString) => database.BlizzardData.Where(r => r.Name.EsEs.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Fr_Fr] = (database, searchString) => database.BlizzardData.Where(r => r.Name.FrFr.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Ru_Ru] = (database, searchString) => database.BlizzardData.Where(r => r.Name.RuRu.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.De_De] = (database, searchString) => database.BlizzardData.Where(r => r.Name.DeDe.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Pt_Pt] = (database, searchString) => database.BlizzardData.Where(r => r.Name.PtPt.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.It_It] = (database, searchString) => database.BlizzardData.Where(r => r.Name.ItIt.ToLower().StartsWith(searchString));

        _searchFunc[(int)ServerSideLocale.Ko_Kr] = (database, searchString) => database.BlizzardData.Where(r => r.Name.KoKr.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Zh_Tw] = (database, searchString) => database.BlizzardData.Where(r => r.Name.ZhTw.ToLower().StartsWith(searchString));
        _searchFunc[(int)ServerSideLocale.Zh_Cn] = (database, searchString) => database.BlizzardData.Where(r => r.Name.ZhCn.ToLower().StartsWith(searchString));

        Exceptions.ThrowIf(_getterFunc.Any(x => x == null));
    }

    public static string GetName(ServerSideLocale locale, BlizzardDataRecordLocal record)
    {
        if (locale < 0 || locale >= ServerSideLocale.Count)
        {
            locale = ServerSideLocale.None;
        }

        return _getterFunc[(int)locale](record);
    }

    public static IQueryable<BlizzardDataRecord> GetSearchQuery(AppDbContext database, ServerSideLocale locale, string searchString)
    {
        return _searchFunc[(int)locale](database, searchString);
    }
}