namespace AzerothMemories.WebBlazor.Services.Commands;

public record Post_TryReactToPostComment(Session Session, long PostId, long CommentId, PostReaction NewReaction) : ICommand<long>
{
    public Post_TryReactToPostComment() : this(Session.Null, 0, 0, PostReaction.None)
    {
    }
}