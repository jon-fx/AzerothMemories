namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryReactToPost(Session Session, int PostId, PostReaction NewReaction) : ISessionCommand<int>;