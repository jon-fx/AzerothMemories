namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Following_TryStartFollowing(Session Session, int OtherAccountId) : ISessionCommand<AccountFollowingStatus?>;