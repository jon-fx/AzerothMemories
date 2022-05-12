namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryDeletePost(Session Session, int PostId) : ISessionCommand<long>;