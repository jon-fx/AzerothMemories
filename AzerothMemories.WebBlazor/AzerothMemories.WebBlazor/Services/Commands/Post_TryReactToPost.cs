namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReactToPost(Session Session, int PostId, PostReaction NewReaction) : ISessionCommand<int>
{
    public Post_TryReactToPost() : this(Session.Null, 0, PostReaction.None)
    {
    }
}