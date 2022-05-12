namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryDisconnectAccount(Session Session, string Schema, string Key) : ISessionCommand<bool>;