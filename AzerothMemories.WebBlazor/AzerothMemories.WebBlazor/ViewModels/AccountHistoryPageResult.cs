namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class AccountHistoryPageResult
{
    [JsonInclude, DataMember, MemoryPackInclude] public int TotalPages { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public int CurrentPage { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public AccountHistoryViewModel[] ViewModels { get; set; } = Array.Empty<AccountHistoryViewModel>();
}