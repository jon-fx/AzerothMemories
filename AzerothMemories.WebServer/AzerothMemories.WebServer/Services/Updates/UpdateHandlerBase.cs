using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Services.Updates;

public class UpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord
{
    private readonly BlizzardUpdateType _updateType;
    private readonly CommonServices _commonServices;
    private readonly ILogger<BlizzardUpdateServices> _logger;
    private readonly string _updateTypeString;

    public UpdateHandlerBase(BlizzardUpdateType updateType, CommonServices commonServices, ILogger<BlizzardUpdateServices> logger, [CallerArgumentExpression("updateType")] string? updateTypeString = null)
    {
        _updateType = updateType;
        _commonServices = commonServices;
        _logger = logger;

        Exceptions.ThrowIf(string.IsNullOrWhiteSpace(updateTypeString));
        Exceptions.ThrowIf(!updateTypeString.StartsWith("BlizzardUpdateType."));

        _updateTypeString = updateTypeString.Replace("BlizzardUpdateType.", "");

        Exceptions.ThrowIf(!Enum.TryParse<BlizzardUpdateType>(_updateTypeString, out var updateEnum));
        Exceptions.ThrowIf(updateEnum != updateType);
    }

    public BlizzardUpdateType UpdateType => _updateType;

    public string UpdateTypeString => _updateTypeString;

    public CommonServices CommonServices => _commonServices;

    protected ILogger<BlizzardUpdateServices> Logger => _logger;

    protected virtual bool ShouldExecuteOn(AppDbContext database, TRecord record, out AuthTokenRecord? authTokenRecord)
    {
        authTokenRecord = null;
        return true;
    }

    public async Task<HttpStatusCode> TryExecuteOn(AppDbContext database, TRecord record, BlizzardUpdateChildRecord childRecord)
    {
        if (ShouldExecuteOn(database, record, out var authTokenRecord))
        {
            return await InternalExecuteOn(database, record, authTokenRecord, childRecord).ConfigureAwait(false);
        }

        return HttpStatusCode.OK;
    }

    protected virtual Task<HttpStatusCode> InternalExecuteOn(AppDbContext database, TRecord record, AuthTokenRecord? authTokenRecord, BlizzardUpdateChildRecord childRecord)
    {
        return Task.FromResult(HttpStatusCode.OK);
    }
}