﻿namespace AzerothMemories.Services;

[BasePath("character")]
public interface ICharacterServices
{
    //[ComputeMethod] [Get(nameof(TryGetAccount) + "/{accountId}")] Task<AccountViewModel> TryGetAccount(Session session, [Path] long accountId, CancellationToken cancellationToken = default);

    //[Post(nameof(TryChangeUsername) + "/{newUsername}")] Task<string> TryChangeUsername(Session session, [Path] string newUsername, CancellationToken cancellationToken = default);
}