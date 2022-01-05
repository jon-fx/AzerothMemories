namespace AzerothMemories.WebBlazor.Services;

public sealed class TagHelpers
{
    public PostTagInfo[] MainTags { get; }

    public PostTagInfo[] CommonTags { get; }

    public TagHelpers(IStringLocalizer<BlizzardResources> stringLocalizer)
    {
        static int GetId(string key)
        {
            int.TryParse(key.Split('-')[1], out var id);
            return id;
        }

        var allTypeTags = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Type-")).ToDictionary(x => GetId(x.Name), x => x.Value);
        var allCommonTags = stringLocalizer.GetAllStrings().Where(x => x.Name.StartsWith("Main-")).ToDictionary(x => GetId(x.Name), x => x.Value);
        
        if (allTypeTags.Count == 0)
        {
            throw new NotImplementedException();
        }

        if (allCommonTags.Count == 0)
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

        MainTags = GetArray(PostTagType.Type, allTypeTags, false);
        CommonTags = GetArray(PostTagType.Main, allCommonTags, true);
    }
}