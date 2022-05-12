namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryChangeUsername(Session Session, int AccountId, string NewUsername) : ISessionCommand<bool>;