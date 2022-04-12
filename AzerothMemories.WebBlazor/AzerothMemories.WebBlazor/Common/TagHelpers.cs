namespace AzerothMemories.WebBlazor.Common;

public sealed class TagHelpers
{
    public PostTagInfo[] TypeTags { get; }
    public PostTagInfo[] RegionTags { get; }
    public PostTagInfo[] CommonTags { get; }

    private readonly Dictionary<string, int> _realmSlugsToId;
    private readonly Dictionary<string, string> _realmNamesToSlugs;

    public TagHelpers(IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        static int GetId(string key)
        {
            int.TryParse(key.Split('-')[1], out var id);
            return id;
        }

        var allTypeTags = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Type-")).ToDictionary(x => GetId(x.Name), x => x.Value);
        var allCommonTags = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Main-")).ToDictionary(x => GetId(x.Name), x => x.Value);
        var allRegionTags = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Region-")).ToDictionary(x => GetId(x.Name), x => x.Value);

        if (allTypeTags.Count == 0 || allCommonTags.Count == 0 || allRegionTags.Count == 0)
        {
            throw new NotImplementedException();
        }

        static PostTagInfo[] GetArray(PostTagType type, Dictionary<int, string> dictionary, bool isChipClosable)
        {
            var allTagInfo = dictionary.Select(x => new PostTagInfo(type, x.Key, x.Value, null) { IsChipClosable = isChipClosable }).ToArray();
            var array = new PostTagInfo[allTagInfo.Max(x => x.Id + 1)];

            foreach (var tag in allTagInfo)
            {
                array[tag.Id] = tag;
            }

            return array;
        }

        TypeTags = GetArray(PostTagType.Type, allTypeTags, false);
        RegionTags = GetArray(PostTagType.Region, allRegionTags, false);
        CommonTags = GetArray(PostTagType.Main, allCommonTags, true);

        var allRealmNames = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Realm-")).ToDictionary(x => GetId(x.Name), x => x.Value);
        var allRealmSlugs = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("RealmSlug-")).ToDictionary(x => GetId(x.Name), x => x.Value);

        //_allValidRealmSlugs = allRealmSlugs.Select(x => x.Value).ToHashSet();
        _realmSlugsToId = new Dictionary<string, int>();
        _realmNamesToSlugs = new Dictionary<string, string>();

        foreach (var realmSlug in allRealmSlugs)
        {
            var id = realmSlug.Key;
            var slug = realmSlug.Value;

            if (allRealmNames.TryGetValue(id, out var name))
            {
                _realmNamesToSlugs[name.ToLower()] = slug;
            }

            _realmSlugsToId[slug] = id;
        }
    }

    public bool GetRealmId(string realmSlug, out int realmId)
    {
        return _realmSlugsToId.TryGetValue(realmSlug, out realmId);
    }

    public bool GetRealmSlug(string realName, out string realmSlug)
    {
        return _realmNamesToSlugs.TryGetValue(realName.ToLowerInvariant(), out realmSlug);
    }
}