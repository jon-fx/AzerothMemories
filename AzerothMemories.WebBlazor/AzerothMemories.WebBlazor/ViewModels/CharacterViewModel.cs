namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class CharacterViewModel
{
    [JsonInclude] public long Id;

    [JsonInclude] public string Ref;

    [JsonInclude] public BlizzardRegion RegionId;

    [JsonInclude] public int RealmId;

    [JsonInclude] public string Name;

    [JsonInclude] public byte Class;

    [JsonInclude] public byte Race;

    [JsonInclude] public byte Gender;

    [JsonInclude] public byte Level;

    [JsonInclude] public bool AccountSync;

    [JsonInclude] public string AvatarLink;

    [JsonInclude] public long GuildId;

    [JsonInclude] public string GuildName;

    [JsonInclude] public HttpStatusCode LastUpdateHttpResult;

    [JsonIgnore]
    public string AvatarLinkWithFallBack
    {
        get
        {
            var str = AvatarLink;
            if (string.IsNullOrEmpty(str))
            {
                str = "https://render-us.worldofwarcraft.com/character/tichondrius/00/000000000-avatar.jpg";
            }

            return $"{str}?alt=/shadow/avatar/{Race}-{Gender}.jpg";
        }
    }
}