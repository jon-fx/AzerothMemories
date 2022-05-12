namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Admin_TryBanUser(Session Session, int AccountId, long Duration, string BanReason) : ISessionCommand<bool>;