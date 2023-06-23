namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class GuildMembersViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public int Index;

    [JsonInclude, DataMember, MemoryPackInclude] public int TotalCount;

    [JsonInclude, DataMember, MemoryPackInclude] public CharacterViewModel[] CharactersArray;
}