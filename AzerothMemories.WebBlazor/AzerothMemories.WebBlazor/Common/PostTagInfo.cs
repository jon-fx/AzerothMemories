namespace AzerothMemories.WebBlazor.Common;

public sealed class PostTagInfo
{
    public static readonly IEqualityComparer<PostTagInfo> EqualityComparer1 = new PostTagInfoEqualityComparer1();
    public static readonly IEqualityComparer<object> EqualityComparer2 = new PostTagInfoEqualityComparer2();

    [JsonInclude] public readonly int Id;
    [JsonInclude] public readonly string Name;
    [JsonInclude] public readonly string Image;
    [JsonInclude] public readonly PostTagType Type;
    [JsonInclude] public readonly long MinTagTime;

    [JsonIgnore] private string _nameWithIcon;
    [JsonIgnore] private string _wowHeadLink;

    public PostTagInfo(PostTagType type, int id, string name, string image, long minTagTime = 0)
    {
        Id = id;
        Image = image;
        Name = name;
        Type = type;
        MinTagTime = minTagTime;
    }

    [JsonInclude] public bool IsChipClosable { get; set; } = true;

    [JsonIgnore] public string NameWithIcon => _nameWithIcon ??= $"{Type.GetTagIcon()}{Name}";

    [JsonIgnore] public string WowHeadLink => _wowHeadLink ??= Type.GetWowHeadLink(Id);

    [JsonIgnore]
    public string TagString
    {
        get
        {
            if (Type == PostTagType.HashTag)
            {
                return $"{Type}-{Name}";
            }

            return GetTagString(Type, Id);
        }
    }

    public string GetTagQueryLink()
    {
        return $"postsearch?tag={TagString}";
    }

    public string GetTagValue()
    {
        if (Type == PostTagType.HashTag)
        {
            return $"{(int)Type}-{Name}";
        }

        return $"{(int)Type}-{Id}";
    }

    public static string GetTagString(PostTagType tagType, int tagId)
    {
        return $"{tagType}-{tagId}";
    }
}