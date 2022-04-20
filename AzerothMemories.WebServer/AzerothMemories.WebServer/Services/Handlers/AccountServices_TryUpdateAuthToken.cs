namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryUpdateAuthToken
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Account_TryUpdateAuthToken command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
            }

            return default;
        }

        var key = $"{command.Type}/{command.Id}";
        var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
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
        record.AccountId ??= command.AccountId;
        record.Token = command.AccessToken;
        record.RefreshToken = command.RefreshToken;
        record.TokenExpiresAt = Instant.FromUnixTimeMilliseconds(command.TokenExpiresAt);
        record.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (record.AccountId.HasValue)
        {
            context.Operation().Items.Set(new Account_InvalidateAccountRecord(record.AccountId.Value, null, null));
        }

        var result = !command.AccountId.HasValue || command.AccountId == record.AccountId;
        return result;
    }
}