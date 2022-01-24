namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryRestoreMemory(Session Session, long PostId, long PreviousCharacterId, long NewCharacterId) : ICommand<bool>
{
    public Post_TryRestoreMemory() : this(Session.Null, 0, 0, 0)
    {
    }
}