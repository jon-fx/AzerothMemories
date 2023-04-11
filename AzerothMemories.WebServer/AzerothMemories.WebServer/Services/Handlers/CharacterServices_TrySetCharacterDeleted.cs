namespace AzerothMemories.WebServer.Services.Handlers;

internal static class CharacterServices_TrySetCharacterDeleted
{
    public static async Task<bool> TryHandle(CommonServices commonServices, Character_TrySetCharacterDeleted command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Character_InvalidateCharacterRecord>();
            if (invRecord != null)
            {
                _ = commonServices.CharacterServices.DependsOnCharacterRecord(invRecord.CharacterId);
            }

            return default;
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var characterRecord = await commonServices.CharacterServices.TryGetCharacterRecord(command.CharacterId).ConfigureAwait(false);
        if (characterRecord == null)
        {
            return false;
        }

        if (characterRecord.AccountId != activeAccount.Id)
        {
            return false;
        }

        if (characterRecord.CharacterStatus != CharacterStatus2.MaybeDeleted)
        {
            return false;
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(characterRecord);
        characterRecord.CharacterStatus = CharacterStatus2.DeletePending;
        characterRecord.NameSearchable = $"Ð-{characterRecord.NameSearchable}";

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Character_InvalidateCharacterRecord(command.CharacterId, characterRecord.AccountId.GetValueOrDefault()));

        return true;
    }
}