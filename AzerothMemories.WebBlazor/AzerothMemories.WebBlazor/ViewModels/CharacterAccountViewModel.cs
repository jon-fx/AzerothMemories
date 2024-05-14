namespace AzerothMemories.WebBlazor.ViewModels;

[DataContract, MemoryPackable]
public sealed partial class CharacterAccountViewModel
{
    [JsonInclude, DataMember, MemoryPackInclude] public AccountViewModel? AccountViewModel { get; set; }
    [JsonInclude, DataMember, MemoryPackInclude] public CharacterViewModel? CharacterViewModel { get; set; }
}