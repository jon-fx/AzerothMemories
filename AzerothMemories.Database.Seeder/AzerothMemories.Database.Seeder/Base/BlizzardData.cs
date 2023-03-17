using NodaTime;
using System.Text.Json.Serialization;

namespace AzerothMemories.Database.Seeder.Base
{
    public sealed class BlizzardData
    {
        public BlizzardData()
        {
        }

        public BlizzardData(PostTagType tagType, int tagId) : this()
        {
            TagId = tagId;
            TagType = tagType;
        }

        [JsonInclude] public int TagId { get; init; }

        [JsonInclude] public PostTagType TagType { get; init; }

        [JsonIgnore] public string Key => $"{TagType}-{TagId}";

        [JsonIgnore] public Instant MinTagTime { get; init; }

        [JsonInclude] public string[] Names { get; init; } = new string[(int)ServerSideLocale.Count];

        [JsonInclude] public string Media { get; set; }

        public bool TryGetNameOrDefault(ServerSideLocale key, out string result)
        {
            if (!string.IsNullOrWhiteSpace(Names[(int)key]))
            {
                result = Names[(int)key];

                return true;
            }

            if (!string.IsNullOrWhiteSpace(Names[(int)ServerSideLocale.En_Us]))
            {
                result = Names[(int)ServerSideLocale.En_Us];

                return true;
            }

            throw new NotImplementedException();
        }

        public string GetNameOrDefault(ServerSideLocale serverSideLocale)
        {
            var current = Names[(int)serverSideLocale];
            if (!string.IsNullOrWhiteSpace(current))
            {
                return current;
            }

            var enUs = Names[(int)ServerSideLocale.En_Us];
            if (!string.IsNullOrWhiteSpace(enUs))
            {
                return enUs;
            }

            return Key;
        }
    }
}