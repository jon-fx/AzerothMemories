namespace AzerothMemories.WebBlazor.Blizzard;

public static class HttpStatusCodeExt
{
    public static RequestResultCode ToResult(this HttpStatusCode statusCode)
    {
        if (IsSuccess(statusCode))
        {
            return RequestResultCode.Success;
        }

        return RequestResultCode.Failed;
    }

    public static bool IsSuccess(this HttpStatusCode statusCode)
    {
        var asInt = (int)statusCode;
        return asInt >= 200 && asInt <= 299;
    }

    public static bool IsFailure(this HttpStatusCode statusCode)
    {
        return !IsSuccess(statusCode);
    }
}