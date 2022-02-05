namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryPostMemory(Session Session, long TimeStamp, string AvatarTag, bool IsPrivate, string Comment, HashSet<string> SystemTags, List<byte[]> ImageData) : ICommand<AddMemoryResult>
{
    public Post_TryPostMemory() : this(Session.Null, 0, null, false, null, null, null)
    {
    }
}