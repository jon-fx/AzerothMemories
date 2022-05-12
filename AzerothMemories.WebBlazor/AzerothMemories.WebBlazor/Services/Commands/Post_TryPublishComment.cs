namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryPublishComment(Session Session, int PostId, int ParentCommentId, string CommentText) : ISessionCommand<int>;