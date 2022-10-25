using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace AzerothMemories.Database.Seeder.Base;

internal static class CsvHelpers
{
    private static readonly CsvConfiguration _config;
    private static readonly List<(Type type, ITypeConverter converter)> _converters;

    static CsvHelpers()
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture);
        _converters = new List<(Type type, ITypeConverter converter)>();

        _config.AllowComments = true;
        _config.ShouldSkipRecord = args => args.Row.Parser.Record == null || args.Row.Parser.Record.All(string.IsNullOrEmpty);
    }

    private static void Initialize(this CsvContext context)
    {
        context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add("NULL");
        context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add(string.Empty);
        context.TypeConverterOptionsCache.GetOptions<bool>().BooleanFalseValues.Add(string.Empty);

        for (var i = 0; i < _converters.Count; i++)
        {
            context.TypeConverterCache.AddConverter(_converters[i].type, _converters[i].converter);
        }
    }

    public static CsvReader GetReader(StreamReader streamReader)
    {
        var reader = new CsvReader(streamReader, _config);
        reader.Context.Initialize();

        return reader;
    }

    //public static CsvWriter GetWriter(StreamWriter streamWriter)
    //{
    //    var writer = new CsvWriter(streamWriter, _config);
    //    writer.Context.Initialize();

    //    return writer;
    //}
}