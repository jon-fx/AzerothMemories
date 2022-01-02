namespace AzerothMemories.WebBlazor.Services;

internal sealed class PostTagInfoEqualityComparer2 : IEqualityComparer<object>
{
    public new bool Equals(object x, object y)
    {
        var postTagInfo1 = x as PostTagInfo;
        var postTagInfo2 = y as PostTagInfo;

        return PostTagInfo.EqualityComparer1.Equals(postTagInfo1, postTagInfo2);
    }

    public int GetHashCode(object obj)
    {
        var postTagInfo1 = (PostTagInfo)obj;

        return PostTagInfo.EqualityComparer1.GetHashCode(postTagInfo1);
    }
}