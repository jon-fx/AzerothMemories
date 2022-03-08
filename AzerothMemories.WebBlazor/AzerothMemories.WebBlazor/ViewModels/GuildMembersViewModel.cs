namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class GuildMembersViewModel
{
    [JsonInclude] public int Index;

    [JsonInclude] public int TotalCount;

    [JsonInclude] public CharacterViewModel[] CharactersArray;
}