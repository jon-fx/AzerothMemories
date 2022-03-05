namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class MainSearchResult
{
    [JsonInclude] public int Id { get; init; }
    [JsonInclude] public string RefStr { get; init; }
    [JsonInclude] public string Name { get; init; }

    [JsonInclude] public string Avatar { get; init; }
    [JsonInclude] public byte CharacterClass { get; init; }
    [JsonInclude] public int RealmId { get; init; }

    [JsonInclude] public MainSearchType Type { get; init; }

    public string GetLink()
    {
        if (Type == MainSearchType.Account)
        {
            return $@"account\{Id}";
        }

        if (Type == MainSearchType.Character)
        {
            var moaRef = new MoaRef(RefStr);
            var region = moaRef.Region.ToInfo();
            var realmSlug = moaRef.Realm;

            return $@"character\{region.TwoLetters}\{realmSlug}\{moaRef.Name}";
        }

        if (Type == MainSearchType.Guild)
        {
            var moaRef = new MoaRef(RefStr);
            var region = moaRef.Region.ToInfo();
            var realmSlug = moaRef.Realm;

            return $@"guild\{region.TwoLetters}\{realmSlug}\{moaRef.Name}";
        }

        throw new NotImplementedException();
    }

    public PostTagInfo ToTagInfo()
    {
        if (Type == MainSearchType.Account)
        {
            return new PostTagInfo(PostTagType.Account, Id, Name, Avatar);
        }

        if (Type == MainSearchType.Character)
        {
            return new PostTagInfo(PostTagType.Character, Id, Name, Avatar);
        }

        if (Type == MainSearchType.Guild)
        {
            return new PostTagInfo(PostTagType.Guild, Id, Name, Avatar);
        }

        throw new NotImplementedException();
    }

    public static MainSearchResult CreateAccount(int id, string name, string avatar)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            Type = MainSearchType.Account
        };
    }

    public static MainSearchResult CreateCharacter(int id, string moaRef, string name, string avatar, int realmId, byte characterClass)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            RefStr = moaRef,
            RealmId = realmId,
            CharacterClass = characterClass,
            Type = MainSearchType.Character
        };
    }

    public static MainSearchResult CreateGuild(int id, string moaRef, string name, string avatar, int realmId)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            RefStr = moaRef,
            RealmId = realmId,
            Type = MainSearchType.Guild
        };
    }
}