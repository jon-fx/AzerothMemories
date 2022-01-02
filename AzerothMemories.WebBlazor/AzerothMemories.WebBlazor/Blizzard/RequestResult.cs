namespace AzerothMemories.WebBlazor.Blizzard;

public sealed class RequestResult<T> where T : class
{
    public readonly T ResultData;
    public readonly string ResultString;
    public readonly HttpStatusCode ResultCode;
    public readonly DateTimeOffset ResultLastModified;

    public RequestResult(HttpStatusCode resultCode, T resultData, DateTimeOffset resultLastModified, string resultString)
    {
        ResultCode = resultCode;
        ResultData = resultData;
        ResultLastModified = resultLastModified;
        ResultString = resultString;
    }

    public bool IsSuccess => ResultCode.IsSuccess();

    public bool IsFailure => ResultCode.IsFailure();

    public bool IsNotModified => ResultCode == HttpStatusCode.NotModified;

    public long ResultLastModifiedMs => ResultLastModified.ToUnixTimeMilliseconds();
}