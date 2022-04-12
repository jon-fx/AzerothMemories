using System.Runtime.CompilerServices;

namespace AzerothMemories.WebServer.Services.Updates;

internal abstract class UpdateHandlerBaseResult<TRecord, TRequestResult> : UpdateHandlerBase<TRecord> where TRecord : IBlizzardUpdateRecord where TRequestResult : class
{
    protected UpdateHandlerBaseResult(BlizzardUpdateType updateType, CommonServices commonServices, [CallerArgumentExpression("updateType")] string updateTypeString = null) : base(updateType, commonServices, updateTypeString)
    {
    }

    protected abstract Task<RequestResult<TRequestResult>> TryExecuteRequest(TRecord record, Instant blizzardLastModified);

    protected override async Task<HttpStatusCode> InternalExecuteOn(CommandContext context, AppDbContext database, TRecord record, BlizzardUpdateChildRecord childRecord)
    {
        var requestResult = await TryExecuteRequest(record, childRecord.BlizzardLastModified).ConfigureAwait(false);
        if (requestResult.IsSuccess)
        {
            childRecord.UpdateFailCounter = 0;

            await InternalExecuteWithResult(context, database, record, requestResult.ResultData).ConfigureAwait(false);
        }
        else if (childRecord.UpdateFailCounter < byte.MaxValue)
        {
            childRecord.UpdateFailCounter++;
        }

        childRecord.UpdateJobLastResult = requestResult.ResultCode;
        childRecord.BlizzardLastModified = requestResult.ResultLastModified;

        return requestResult.ResultCode;
    }

    protected abstract Task InternalExecuteWithResult(CommandContext context, AppDbContext database, TRecord record, TRequestResult requestResult);
}