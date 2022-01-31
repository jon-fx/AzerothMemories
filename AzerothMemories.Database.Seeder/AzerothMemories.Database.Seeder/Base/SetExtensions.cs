using System.Reflection;

namespace AzerothMemories.Database.Seeder.Base;

internal static class SetExtensions
{
    public static BlizzardDataRecordLocal ToRecord(this Name name)
    {
        return new BlizzardDataRecordLocal
        {
            EnUs = name.En_US,
            KoKr = name.Ko_KR,
            FrFr = name.Fr_FR,
            DeDe = name.De_DE,
            ZhCn = name.Zh_CN,
            EsEs = name.Es_ES,
            ZhTw = name.Zh_TW,
            EnGb = name.En_GB,
            EsMx = name.Es_MX,
            RuRu = name.Ru_RU,
            PtBr = name.Pt_BR,
            ItIt = name.It_IT,
            PtPt = name.Pt_PT,
        };
    }

    public static void SetValue(BlizzardDataRecordLocal record, string key, string value)
    {
        var index = key.IndexOf('_') + 1;
        var fieldName = key[index..];
        var fieldInfo = typeof(BlizzardDataRecordLocal).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
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
                fieldValue = record.EnGb;

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
        var fields = typeof(BlizzardDataRecordLocal).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var fieldName = field.Name.ToLower();
            if (fieldName.Length != 5)
            {
                continue;
            }

            var fieldInfo = typeof(BlizzardDataRecordLocal).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
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