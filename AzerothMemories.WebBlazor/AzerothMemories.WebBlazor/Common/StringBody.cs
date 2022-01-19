namespace AzerothMemories.WebBlazor.Common;

public sealed class StringBody
{
    [JsonInclude] public string Value;

    public StringBody()
    {
    }

    public StringBody(string value)
    {
        Value = value;
    }
}