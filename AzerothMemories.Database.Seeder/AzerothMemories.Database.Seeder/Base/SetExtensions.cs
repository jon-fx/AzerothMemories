using ServerSideLocale = AzerothMemories.WebBlazor.Common.ServerSideLocale;

namespace AzerothMemories.Database.Seeder.Base;

internal static class SetExtensions
{
    public static string[] ToArray(this Name name)
    {
        var results = new string[(int)ServerSideLocale.Count];

        results[(int)ServerSideLocale.En_Us] = name.En_US;
        results[(int)ServerSideLocale.Es_Mx] = name.Es_MX;
        results[(int)ServerSideLocale.Pt_Br] = name.Pt_BR;
        results[(int)ServerSideLocale.En_Gb] = name.En_GB;

        results[(int)ServerSideLocale.Es_Es] = name.Es_ES;
        results[(int)ServerSideLocale.Fr_Fr] = name.Fr_FR;
        results[(int)ServerSideLocale.Ru_Ru] = name.Ru_RU;
        results[(int)ServerSideLocale.De_De] = name.De_DE;

        results[(int)ServerSideLocale.Pt_Pt] = name.Pt_PT;
        results[(int)ServerSideLocale.It_It] = name.It_IT;

        results[(int)ServerSideLocale.Ko_Kr] = name.Ko_KR;
        results[(int)ServerSideLocale.Zh_Tw] = name.Zh_TW;
        results[(int)ServerSideLocale.Zh_Cn] = name.Zh_CN;

        return results;
    }

    public static void Update(string[] dataDictionary, Func<ServerSideLocale, string, string> func)
    {
        var defaultValue = dataDictionary[(int)ServerSideLocale.En_Us];
        if (string.IsNullOrWhiteSpace(defaultValue))
        {
            throw new NotImplementedException();
        }

        for (var i = 0; i < (int)ServerSideLocale.Count; i++)
        {
            var currentValue = dataDictionary[i];
            if (string.IsNullOrWhiteSpace(currentValue))
            {
                currentValue = defaultValue;
            }

            var newValue = func((ServerSideLocale)i, currentValue);
            dataDictionary[i] = newValue;
        }
    }
}