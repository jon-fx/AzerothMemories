namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TrySetPostVisibility(Session Session, int PostId, byte NewVisibility) : ISessionCommand<byte?>;