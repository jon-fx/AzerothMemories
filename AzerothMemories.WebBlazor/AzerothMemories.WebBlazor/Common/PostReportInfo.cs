namespace AzerothMemories.WebBlazor.Common;

public class PostReportInfo
{
    [JsonInclude] public string ReasonText { get; init; }
    [JsonInclude] public PostReportedReason Reason { get; init; }
}