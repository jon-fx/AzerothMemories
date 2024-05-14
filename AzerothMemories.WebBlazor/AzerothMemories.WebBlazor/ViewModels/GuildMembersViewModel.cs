namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class GuildMembersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public required int Index { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public required int TotalCount { get; init; }

    [JsonInclude, DataMember, MemoryPackInclude] public required CharacterViewModel[] CharactersArray { get; init; }
}