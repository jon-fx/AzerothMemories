namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class ReportedChildViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo UserTag { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostTagInfo ReportedTag { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int RecordId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int ReportedTagId { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public PostReportedReason Reason { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public string ReasonText { get; init; }
}