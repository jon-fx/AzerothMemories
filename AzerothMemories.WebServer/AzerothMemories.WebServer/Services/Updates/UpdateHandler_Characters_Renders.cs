namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Characters_Renders : UpdateHandlerBase<CharacterRecord, CharacterMediaSummary>
{
    public UpdateHandler_Characters_Renders(CommonServices commonServices) : base(BlizzardUpdateType.Character_Renders, commonServices)
    {
    }

    protected override async Task<RequestResult<CharacterMediaSummary>> TryExecuteRequest(CharacterRecord record, Instant blizzardLastModified)
    {
        var characterRef = new MoaRef(record.MoaRef);
        using var client = CommonServices.WarcraftClientProvider.Get(record.BlizzardRegionId);
        return await client.GetCharacterRendersAsync(characterRef.Realm, characterRef.Name, blizzardLastModified).ConfigureAwait(false);
    }

    protected override Task InternalExecute(CommandContext context, AppDbContext database, CharacterRecord record, CharacterMediaSummary requestResult)
    {
        var assets = requestResult.Assets;
        var characterAvatarRender = record.AvatarLink;
        if (assets != null)
        {
            var avatar = assets.FirstOrDefault(x => x.Key == "avatar");
            characterAvatarRender = avatar?.Value.AbsoluteUri;
        }

        record.AvatarLink = characterAvatarRender;

        return Task.CompletedTask;
    }
}