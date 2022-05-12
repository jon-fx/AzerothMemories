namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryRestoreMemory(Session Session, int PostId, int NewCharacterId) : ISessionCommand<bool>;