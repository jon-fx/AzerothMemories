namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryRestoreMemory(Session Session, int PostId, int NewCharacterId) : ISessionCommand<bool>
{
    public Post_TryRestoreMemory() : this(Session.Null, 0, 0)
    {
    }
}