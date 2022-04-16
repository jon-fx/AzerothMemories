namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class AccountViewModelLinks
{
    [JsonInclude] public int Id { get; set; }

    [JsonInclude] public string Name { get; set; }

    [JsonInclude] public string Key { get; set; }
}