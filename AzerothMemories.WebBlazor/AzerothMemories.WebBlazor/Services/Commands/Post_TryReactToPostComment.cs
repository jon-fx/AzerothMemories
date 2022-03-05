namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReactToPostComment(Session Session, int PostId, int CommentId, PostReaction NewReaction) : ISessionCommand<int>
{
    public Post_TryReactToPostComment() : this(Session.Null, 0, 0, PostReaction.None)
    {
    }
}