namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class BlizzardUpdateViewModel
{
    [JsonInclude] public long UpdateLastModified;

    [JsonInclude] public long UpdateJobLastEndTime;

    [JsonInclude] public HttpStatusCode UpdateJobLastResult;

    [JsonInclude] public BlizzardUpdateViewModelChild[] Children;

    [JsonIgnore] public bool IsLoadingFromArmory => UpdateJobLastEndTime == 0;
}