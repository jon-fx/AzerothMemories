namespace AzerothMemories.Services;

public class AccountViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public string Ref;

    [JsonInclude] public string Username;

    [JsonInclude] public string BattleTag;

    [JsonInclude] public string Avatar;

    [JsonInclude] public DateTimeOffset CreatedDateTime;

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

public sealed class ActiveAccountViewModel : AccountViewModel
{
}