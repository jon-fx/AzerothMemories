namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountHistoryPageViewModel
{
    [JsonInclude] public int TotalPages { get; set; }
    [JsonInclude] public int CurrentPage { get; set; }
    [JsonInclude] public AccountHistoryViewModel[] ViewModels { get; set; } = Array.Empty<AccountHistoryViewModel>();
}