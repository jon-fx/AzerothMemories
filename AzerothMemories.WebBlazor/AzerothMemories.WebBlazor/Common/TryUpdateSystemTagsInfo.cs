namespace AzerothMemories.WebBlazor.Common;

public sealed class TryUpdateSystemTagsInfo
{
    [JsonInclude] public string AvatarText { get; init; }
    [JsonInclude] public HashSet<string> NewTags { get; init; }
}