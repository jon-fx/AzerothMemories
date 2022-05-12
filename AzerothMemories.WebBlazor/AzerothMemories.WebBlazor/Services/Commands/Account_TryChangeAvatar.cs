namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryChangeAvatar(Session Session, int AccountId, string NewAvatar) : ISessionCommand<string>;