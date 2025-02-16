namespace AzerothMemories.WebServer.Services.Updates;

public class UpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord
{
    private readonly BlizzardUpdateType _updateType;
    private readonly CommonServices _commonServices;
    private readonly ILogger<BlizzardUpdateServices> _logger;
    private readonly string _updateTypeString;

    public UpdateHandlerBase(UpdateHandlerInfo handlerInfo)
    {
        _updateType = handlerInfo.UpdateType;
        _commonServices = handlerInfo.CommonServices;
        _logger = handlerInfo.Logger;

        Exceptions.ThrowIf(string.IsNullOrWhiteSpace(handlerInfo.UpdateTypeString));
        Exceptions.ThrowIf(!handlerInfo.UpdateTypeString.StartsWith("BlizzardUpdateType."));

        _updateTypeString = handlerInfo.UpdateTypeString.Replace("BlizzardUpdateType.", "");

        Exceptions.ThrowIf(!Enum.TryParse<BlizzardUpdateType>(_updateTypeString, out var updateEnum));
        Exceptions.ThrowIf(updateEnum != handlerInfo.UpdateType);
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