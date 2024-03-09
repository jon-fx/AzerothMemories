namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class GuildMembersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Index { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterViewModel[] CharactersArray { get; init; }
}