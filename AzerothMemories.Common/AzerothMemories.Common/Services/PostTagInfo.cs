namespace AzerothMemories.Services;

public sealed class PostTagInfo
{
    public static readonly IEqualityComparer<PostTagInfo> EqualityComparer1 = new PostTagInfoEqualityComparer1();
    public static readonly IEqualityComparer<object> EqualityComparer2 = new PostTagInfoEqualityComparer2();

    [JsonInclude] public readonly long Id;
    [JsonInclude] public readonly string Name;
    [JsonInclude] public readonly string Image;
    [JsonInclude] public readonly PostTagType Type;
    //public readonly string NameWithIcon;

    public PostTagInfo(PostTagType type, long id, string name, string image)
    {
        Id = id;
        Image = image;
        Name = name;
        Type = type;
        //NameWithIcon = $"{Type.GetTagIcon()}{Name}";
        //WowHeadLink = Type.GetWowHeadLink(id);
        //ReportCounter = reportCounter;
    }
}