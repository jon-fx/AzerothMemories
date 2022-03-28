namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class ReportedChildViewModel
{
    [JsonInclude] public PostTagInfo UserTag { get; init; }

    [JsonInclude] public PostTagInfo ReportedTag { get; init; }

    [JsonInclude] public int RecordId { get; init; }

    [JsonInclude] public int ReportedTagId { get; init; }

    [JsonInclude] public PostReportedReason Reason { get; init; }

    [JsonInclude] public string ReasonText { get; init; }
}