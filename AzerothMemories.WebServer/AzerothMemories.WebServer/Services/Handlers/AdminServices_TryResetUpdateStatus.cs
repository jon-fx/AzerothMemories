using AzerothMemories.WebServer.Database.Records;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AdminServices_TryResetUpdateStatus
{
    public static async Task<bool> TryHandle(ILogger<AdminServices> logger, CommonServices commonServices, Updates_TryResetUpdateStatus command, CancellationToken cancellationToken)
    {
        if (Invalidation.IsActive)
        {
            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (!activeAccount.IsAdmin())
        {
            return false;
        }

        using var __ = new MethodTimeLogger(logger, new { command.UpdateRecordId }.ToString());
        await using var database = await commonServices.DatabaseHub.CreateOperationDbContext(cancellationToken).ConfigureAwait(false);
        var record = await database.BlizzardUpdates.FirstOrDefaultAsync(x => x.Id == command.UpdateRecordId, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return default;
        }

        var result = await commonServices.Commander.Call(new Updates_UpdateRecordResetStatusCommand(record.AccountId, record.CharacterId, record.GuildId), cancellationToken: cancellationToken).ConfigureAwait(false);
        return result == HttpStatusCode.OK;
    }
}