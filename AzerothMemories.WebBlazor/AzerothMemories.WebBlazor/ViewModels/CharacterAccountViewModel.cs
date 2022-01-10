namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class CharacterAccountViewModel
{
    [JsonInclude] public AccountViewModel AccountViewModel;
    [JsonInclude] public CharacterViewModel CharacterViewModel;
}