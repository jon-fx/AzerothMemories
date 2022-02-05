namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryUpdateSystemTags(Session Session, long PostId, string Avatar, HashSet<string> NewTags) : ISessionCommand<AddMemoryResultCode>
{
    public Post_TryUpdateSystemTags() : this(Session.Null, 0, null, null)
    {
    }
}