namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class BlizzardUpdateViewModelChild
{
    [JsonInclude] public long Id;

    [JsonInclude] public byte UpdateType { get; set; }

    [JsonInclude] public string UpdateTypeString { get; set; }

    [JsonInclude] public HttpStatusCode UpdateJobLastResult;
}