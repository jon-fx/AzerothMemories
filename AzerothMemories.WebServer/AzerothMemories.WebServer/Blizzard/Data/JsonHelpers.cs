namespace AzerothMemories.Blizzard.Data
{
    internal static class JsonHelpers
    {
        public static readonly JsonSerializerOptions JsonSerializerOptions;

        static JsonHelpers()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                //JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
                //JsonSerializerOptions.Converters.Add(new MillisecondTimeSpanConverter());
                //JsonSerializerOptions.Converters.Add(new EpochConverter());
                PropertyNameCaseInsensitive = true
            };
        }
    }
}