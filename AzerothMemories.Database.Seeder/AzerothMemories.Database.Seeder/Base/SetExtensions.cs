using System.Reflection;

namespace AzerothMemories.Database.Seeder.Base;

internal static class SetExtensions
{
    public static BlizzardDataRecordLocal ToRecord(this Name name)
    {
        return new BlizzardDataRecordLocal
        {
            En_Us = name.En_US,
            Ko_Kr = name.Ko_KR,
            Fr_Fr = name.Fr_FR,
            De_De = name.De_DE,
            Zh_Cn = name.Zh_CN,
            Es_Es = name.Es_ES,
            Zh_Tw = name.Zh_TW,
            En_Gb = name.En_GB,
            Es_Mx = name.Es_MX,
            Ru_Ru = name.Ru_RU,
            Pt_Br = name.Pt_BR,
            It_It = name.It_IT,
            Pt_Pt = name.Pt_PT,
        };
    }

    public static void SetValue(BlizzardDataRecordLocal record, string key, string value)
    {
        var index = key.IndexOf('_') + 1;
        var fieldName = key[index..].Insert(2, "_");
        var fieldInfo = typeof(BlizzardDataRecordLocal).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        fieldInfo.SetValue(record, value);
    }

    public static void Update(string key, BlizzardDataRecordLocal record, Dictionary<string, Dictionary<string, string>> clientSideResourcesByLocal)
    {
        var fields = typeof(BlizzardDataRecordLocal).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var fieldName = field.Name.ToLower();
            if (fieldName.Length != 5)
            {
                continue;
            }

            var fieldInfo = typeof(BlizzardDataRecordLocal).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var fieldValue = fieldInfo.GetValue(record) as string;
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                fieldValue = record.En_Gb;

                Console.WriteLine("Set Extensions lang field == null using en_GB");
            }

            if (!clientSideResourcesByLocal.TryGetValue(fieldName, out var dict))
            {
                clientSideResourcesByLocal[fieldName] = dict = new Dictionary<string, string>();
            }

            //if (func != null)
            //{
            //    fieldValue = func(fieldName, fieldValue);
            //}

            dict[key] = fieldValue;
        }
    }

    public static void Update(BlizzardDataRecordLocal record, Func<string, string, string> func)
    {
        var fields = typeof(BlizzardDataRecordLocal).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var fieldName = field.Name.ToLower();
            if (fieldName.Length != 5)
            {
                continue;
            }

            var fieldInfo = typeof(BlizzardDataRecordLocal).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var fieldValue = fieldInfo.GetValue(record) as string;
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                continue;
            }

            //if (!clientSideResourcesByLocal.TryGetValue(fieldName, out var dict))
            //{
            //    clientSideResourcesByLocal[fieldName] = dict = new Dictionary<string, string>();
            //}

            if (func != null)
            {
                fieldValue = func(fieldName, fieldValue);
            }

            fieldInfo.SetValue(record, fieldValue);
        }
    }
}