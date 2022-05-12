namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryReactToPostComment(Session Session, int PostId, int CommentId, PostReaction NewReaction) : ISessionCommand<int>;