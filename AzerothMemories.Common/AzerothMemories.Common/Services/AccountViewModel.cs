namespace AzerothMemories.Services;

public class AccountViewModel
{
    [JsonInclude] public long Id;

    //[JsonInclude] public string Ref;

    [JsonInclude] public string Username;

    [JsonInclude] public string BattleTag;

    [JsonInclude] public bool BattleTagIsPublic;

    [JsonInclude] public bool IsPrivate;

    [JsonInclude] public string Avatar;

    [JsonInclude] public DateTimeOffset CreatedDateTime;

    [JsonInclude] public CharacterViewModel[] CharactersArray = Array.Empty<CharacterViewModel>();

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            return Username;
        }

        if (!string.IsNullOrWhiteSpace(BattleTag))
        {
            return BattleTag;
        }

        return "Unknown";
    }

    public string GetAvatarText()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            return Username[0].ToString();
        }

        if (!string.IsNullOrWhiteSpace(BattleTag))
        {
            return BattleTag[0].ToString();
        }

        return "?";
    }
}