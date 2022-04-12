namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryUpdateAuthToken
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Account_TryUpdateAuthToken command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            //var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            //if (invRecord != null)
            //{
            //    _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
            //    _ = commonServices.AccountServices.TryGetAccountRecordUsername(invRecord.Username);
            //    _ = commonServices.AccountServices.TryGetAccountRecordFusionId(invRecord.FusionId);

            //    _ = commonServices.AdminServices.GetAccountCount();
            //    _ = commonServices.AdminServices.GetSessionCount();
            //}

            return default;
        }

        var key = $"{command.Type}/{command.Id}";
        var database = await databaseContextProvider.CreateCommandDbContextNow(cancellationToken).ConfigureAwait(false);
        var record = await database.AuthTokens.FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            record = new AuthTokenRecord
            {
                Key = key,
            };

            database.AuthTokens.Add(record);
        }

        record.Name = command.Name;
        record.Token = command.AccessToken;
        record.RefreshToken = command.RefreshToken;
        record.TokenExpiresAt = Instant.FromUnixTimeMilliseconds(command.TokenExpiresAt);
        record.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}