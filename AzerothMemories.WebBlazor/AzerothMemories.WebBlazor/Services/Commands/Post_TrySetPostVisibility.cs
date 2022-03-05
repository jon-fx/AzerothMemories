namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TrySetPostVisibility(Session Session, int PostId, byte NewVisibility) : ISessionCommand<byte?>
{
    public Post_TrySetPostVisibility() : this(Session.Null, 0, 0)
    {
    }
}