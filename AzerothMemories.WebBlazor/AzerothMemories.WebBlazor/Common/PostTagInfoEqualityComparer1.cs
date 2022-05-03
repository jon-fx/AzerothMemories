namespace AzerothMemories.WebBlazor.Common;

internal sealed class PostTagInfoEqualityComparer1 : IEqualityComparer<PostTagInfo>
{
    public bool Equals(PostTagInfo x, PostTagInfo y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        if (x.Type == PostTagType.HashTag)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        return x.Id == y.Id && x.Type == y.Type;
    }

    public int GetHashCode(PostTagInfo obj)
    {
        if (obj.Type == PostTagType.HashTag)
        {
            return HashCode.Combine(obj.Id, (int)obj.Type, obj.Name);
        }

        return HashCode.Combine(obj.Id, (int)obj.Type);
    }
}