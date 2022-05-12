namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Following_TryStopFollowing(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>;