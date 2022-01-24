namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReactToPost(Session Session, long PostId, PostReaction NewReaction) : ICommand<long>
{
    public Post_TryReactToPost() : this(Session.Null, 0, PostReaction.None)
    {
    }
}