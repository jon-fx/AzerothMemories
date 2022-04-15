namespace AzerothMemories.WebBlazor.Blizzard;

public static class HttpStatusCodeExt
{
    public static bool IsSuccess(this HttpStatusCode statusCode)
    {
        var asInt = (int)statusCode;
        return asInt >= 200 && asInt <= 299;
    }

    public static bool IsSuccess2(this HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.NotModified)
        {
            return true;
        }

        return statusCode.IsSuccess();
    }
}