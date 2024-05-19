namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class BlizzardUpdateHandler
{
    private readonly CommonServices _commonServices;
    private readonly ILogger<BlizzardUpdateHandler> _logger;
    private readonly BlizzardUpdateServices _blizzardUpdateServices;

    public BlizzardUpdateHandler(CommonServices commonServices, ILogger<BlizzardUpdateHandler> logger, BlizzardUpdateServices blizzardUpdateServices)
    {
        _logger = logger;
        _commonServices = commonServices;
        _blizzardUpdateServices = blizzardUpdateServices;
    }

    public async Task TryUpdate(AccountRecord accountRecord)
    {
        var forcedUpdate = false;
        if (accountRecord.UpdateRecord != null && accountRecord.UpdateRecord.UpdateStatus == BlizzardUpdateStatus.None && accountRecord.AuthTokens != null && accountRecord.AuthTokens.Count > 0)
        {
            var mostRecentlyChanged = accountRecord.AuthTokens.Max(x => x.LastUpdateTime);
            var authTokensChanged = mostRecentlyChanged > accountRecord.UpdateRecord.UpdateJobLastEndTime;
            if (authTokensChanged)
            {
                forcedUpdate = true;
            }
        }

        await TryUpdate(accountRecord, new Updates_UpdateRecordCommand(accountRecord.Id, null, null, forcedUpdate, _blizzardUpdateServices.AccountHandlerCount)).ConfigureAwait(false);
    }

    public async Task TryUpdate(CharacterRecord characterRecord)
    {
        await TryUpdate(characterRecord, new Updates_UpdateRecordCommand(null, characterRecord.Id, null, false, _blizzardUpdateServices.CharacterHandlerCount)).ConfigureAwait(false);
    }

    public async Task TryUpdate(GuildRecord guildRecord)
    {
        await TryUpdate(guildRecord, new Updates_UpdateRecordCommand(null, null, guildRecord.Id, false, _blizzardUpdateServices.GuildHandlerCount)).ConfigureAwait(false);
    }

    private async Task TryUpdate<TRecord>(TRecord? record, Updates_UpdateRecordCommand updateCommand) where TRecord : class, IBlizzardUpdateRecord, new()
    {
        if (record == null)
        {
            return;
        }

        if (record.UpdateRecord == null || record.UpdateRecord.UpdateStatus == BlizzardUpdateStatus.None)
        {
            await _commonServices.Commander.Call(updateCommand).ConfigureAwait(false);
        }
    }
}