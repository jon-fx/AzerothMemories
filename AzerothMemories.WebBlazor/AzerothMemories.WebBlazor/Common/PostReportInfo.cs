namespace AzerothMemories.WebBlazor.Common;

public sealed class PostReportInfo
{
    [JsonInclude] public string ReasonText { get; init; }
    [JsonInclude] public PostReportedReason Reason { get; init; }
}