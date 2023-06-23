namespace AzerothMemories.WebBlazor.Common;

[DataContract, MemoryPackable]
public sealed partial class PostTagInfo
{
    public static readonly IEqualityComparer<PostTagInfo> EqualityComparer1 = new PostTagInfoEqualityComparer1();
    public static readonly IEqualityComparer<object> EqualityComparer2 = new PostTagInfoEqualityComparer2();

    [JsonInclude, DataMember, MemoryPackInclude] public readonly int Id;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly string Name;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly string Image;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly PostTagType Type;
    [JsonInclude, DataMember, MemoryPackInclude] public readonly long MinTagTime;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] private string _nameWithIcon;
    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] private string _wowHeadLink;

    public PostTagInfo(PostTagType type, int id, string name, string image, long minTagTime = 0)
    {
        Id = id;
        Image = image;
        Name = name;
        Type = type;
        MinTagTime = minTagTime;
    }

    [JsonInclude, DataMember, MemoryPackInclude] public bool IsChipClosable { get; set; } = true;

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public string NameWithIcon => _nameWithIcon ??= $"{Type.GetTagIcon()}{Name}";

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore] public string WowHeadLink => _wowHeadLink ??= Type.GetWowHeadLink(Id);

    [JsonIgnore, IgnoreDataMember, MemoryPackIgnore]
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