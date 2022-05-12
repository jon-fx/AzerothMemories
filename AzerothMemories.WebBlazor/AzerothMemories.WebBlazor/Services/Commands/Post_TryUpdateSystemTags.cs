namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryUpdateSystemTags(Session Session, int PostId, string Avatar, HashSet<string> NewTags) : ISessionCommand<AddMemoryResultCode>
{
    public const string DefaultAvatar = "*USE-DEFAULT*";
}