namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryChangeIsPrivate(Session Session, bool NewValue) : ISessionCommand<bool>;