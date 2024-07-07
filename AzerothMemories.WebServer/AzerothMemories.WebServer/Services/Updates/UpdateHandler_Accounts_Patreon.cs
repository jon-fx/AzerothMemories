using System.Diagnostics.CodeAnalysis;

namespace AzerothMemories.WebServer.Services.Updates;

internal sealed class UpdateHandler_Accounts_Patreon : UpdateHandlerBase<AccountRecord>
{
    public UpdateHandler_Accounts_Patreon(CommonServices commonServices, ILogger<BlizzardUpdateServices> logger) : base(BlizzardUpdateType.Account_Patreon, commonServices, logger)
    {
    }

    protected override bool ShouldExecuteOn(CommandContext context, AppDbContext database, AccountRecord record, [NotNullWhen(true)] out AuthTokenRecord? authTokenRecord)
    {
        authTokenRecord = record.AuthTokens.FirstOrDefault(x => x.IsPatreon);
        return authTokenRecord != null;
    }
}