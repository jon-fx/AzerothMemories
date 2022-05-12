namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryDeleteComment(Session Session, int PostId, int CommentId) : ISessionCommand<long>;