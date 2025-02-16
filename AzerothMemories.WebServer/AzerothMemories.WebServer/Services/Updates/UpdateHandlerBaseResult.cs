namespace AzerothMemories.WebServer.Services.Updates;

internal abstract class UpdateHandlerBaseResult<TRecord, TRequestResult> : UpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord where TRequestResult : class
{
    protected UpdateHandlerBaseResult(UpdateHandlerInfo handlerInfo) : base(handlerInfo)
    {
    }

    protected abstract Task<RequestResult<TRequestResult>> TryExecuteRequest(TRecord record, AuthTokenRecord? authTokenRecord, Instant blizzardLastModified);

    protected override async Task<HttpStatusCode> InternalExecuteOn(AppDbContext database, TRecord record, AuthTokenRecord? authTokenRecord, BlizzardUpdateChildRecord childRecord)
    {
        var requestResult = await TryExecuteRequest(record, authTokenRecord, childRecord.BlizzardLastModified).ConfigureAwait(false);
        if (requestResult.IsSuccess && requestResult.ResultData != null)
        {
            childRecord.UpdateFailCounter = 0;

            await InternalExecuteWithResult(database, record, requestResult.ResultData).ConfigureAwait(false);
        }
        else if (childRecord.UpdateFailCounter < byte.MaxValue)
        {
            childRecord.UpdateFailCounter++;
        }

        childRecord.UpdateJobLastResult = requestResult.ResultCode;
        childRecord.BlizzardLastModified = requestResult.ResultLastModified;

        return requestResult.ResultCode;
    }

    protected abstract Task InternalExecuteWithResult(AppDbContext database, TRecord record, TRequestResult requestResult);
}