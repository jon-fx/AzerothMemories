using Microsoft.AspNetCore.WebUtilities;

namespace AzerothMemories.WebBlazor.Services;

public sealed class PostTagInfo
{
    public static readonly IEqualityComparer<PostTagInfo> EqualityComparer1 = new PostTagInfoEqualityComparer1();
    public static readonly IEqualityComparer<object> EqualityComparer2 = new PostTagInfoEqualityComparer2();

    [JsonInclude] public readonly long Id;
    [JsonInclude] public readonly string Name;
    [JsonInclude] public readonly string Image;
    [JsonInclude] public readonly PostTagType Type;
    [JsonInclude] public readonly string NameWithIcon;
    [JsonInclude] public readonly string WowHeadLink;
    [JsonInclude] public int ReportCounter;

    public PostTagInfo(PostTagType type, long id, string name, string image, int reportCounter = 0)
    {
        Id = id;
        Image = image;
        Name = name;
        Type = type;
        NameWithIcon = $"{Type.GetTagIcon()}{Name}";
        WowHeadLink = Type.GetWowHeadLink(id);
        ReportCounter = reportCounter;
    }

    [JsonInclude] public bool IsChipClosable { get; init; } = true;

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
        if (Type == PostTagType.HashTag)
        {
            throw new NotImplementedException();
        }

        return QueryHelpers.AddQueryString("postsearch", "tag", GetTagValue());
    }

    public string GetTagValue()
    {
        if (Type == PostTagType.HashTag)
        {
            return $"{(int)Type}-{Name}";
        }

        return $"{(int)Type}-{Id}";
    }

    public static string GetTagString(PostTagType tagType, long tagId)
    {
        return $"{tagType}-{tagId}";
    }
}