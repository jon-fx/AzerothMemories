namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReportPostTags(Session Session, long PostId, HashSet<string> TagStrings) : ISessionCommand<bool>
{
    public Post_TryReportPostTags() : this(Session.Null, 0, null)
    {
    }
}