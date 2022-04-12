using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Services.Updates;

internal class UpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord
{
    private readonly BlizzardUpdateType _updateType;
    private readonly CommonServices _commonServices;
    private readonly string _updateTypeString;

    public UpdateHandlerBase(BlizzardUpdateType updateType, CommonServices commonServices, [CallerArgumentExpression("updateType")] string updateTypeString = null)
    {
        _updateType = updateType;
        _commonServices = commonServices;

        Exceptions.ThrowIf(string.IsNullOrWhiteSpace(updateTypeString));
        Exceptions.ThrowIf(!updateTypeString.StartsWith("BlizzardUpdateType."));

        _updateTypeString = updateTypeString.Replace("BlizzardUpdateType.", "");

        Exceptions.ThrowIf(!Enum.TryParse<BlizzardUpdateType>(_updateTypeString, out var updateEnum));
        Exceptions.ThrowIf(updateEnum != updateType);
    }

    public BlizzardUpdateType UpdateType => _updateType;

    public string UpdateTypeString => _updateTypeString;

    public CommonServices CommonServices => _commonServices;

    public Task<HttpStatusCode> ExecuteOn(CommandContext context, AppDbContext database, TRecord record, BlizzardUpdateChildRecord childRecord)
    {
        return InternalExecuteOn(context, database, record, childRecord);
    }

    protected virtual Task<HttpStatusCode> InternalExecuteOn(CommandContext context, AppDbContext database, TRecord record, BlizzardUpdateChildRecord childRecord)
    {
        return Task.FromResult(HttpStatusCode.OK);
    }
}