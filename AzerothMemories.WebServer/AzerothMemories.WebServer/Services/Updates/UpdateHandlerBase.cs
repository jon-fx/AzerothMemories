namespace AzerothMemories.WebServer.Services.Updates;

internal abstract class UpdateHandlerBase<TRecord, TRequestResult> : IUpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord where TRequestResult : class
{
    private readonly BlizzardUpdateType _updateType;
    private readonly CommonServices _commonServices;

    protected UpdateHandlerBase(BlizzardUpdateType updateType, CommonServices commonServices)
    {
        _updateType = updateType;
        _commonServices = commonServices;
    }

    public BlizzardUpdateType UpdateType => _updateType;

    public CommonServices CommonServices => _commonServices;

    protected abstract Task<RequestResult<TRequestResult>> TryExecuteRequest(TRecord record, Instant blizzardLastModified);

    public async Task<HttpStatusCode> ExecuteOn(CommandContext context, AppDbContext database, TRecord record, BlizzardUpdateChildRecord updateChildRecord)
    {
        var requestResult = await TryExecuteRequest(record, updateChildRecord.BlizzardLastModified).ConfigureAwait(false);
        if (requestResult.IsSuccess)
        {
            updateChildRecord.UpdateFailCounter = 0;

            await InternalExecute(context, database, record, requestResult.ResultData).ConfigureAwait(false);
        }
        else
        {
            updateChildRecord.UpdateFailCounter++;
        }

        updateChildRecord.UpdateJobLastResult = requestResult.ResultCode;
        updateChildRecord.BlizzardLastModified = requestResult.ResultLastModified;

        return requestResult.ResultCode;
    }

    protected abstract Task InternalExecute(CommandContext context, AppDbContext database, TRecord record, TRequestResult requestResult);
}