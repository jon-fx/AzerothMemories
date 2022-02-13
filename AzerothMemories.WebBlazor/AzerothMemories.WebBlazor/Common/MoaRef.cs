namespace AzerothMemories.WebBlazor.Common;

public sealed class MoaRef
{
    private MoaRef(char type, BlizzardRegion region, string realm, string name, long id)
    {
        Id = id;
        Type = type;
        Name = name.Replace(' ', '-');
        Realm = realm;
        Region = region;

        Full = $"{Type}|{Region.ToValue()}|{Id}|{Realm}|{Name}".ToLower();
    }

    public MoaRef(string full)
    {
        Full = full.ToLower();

        var split = Full.Split('|');
        if (!byte.TryParse(split[1], out var regionId))
        {
            throw new NotImplementedException();
        }

        if (!long.TryParse(split[2], out var id))
        {
            throw new NotImplementedException();
        }

        Id = id;
        Type = split[0][0];
        Name = split[4];
        Realm = split[3];
        Region = (BlizzardRegion)regionId;

        Exceptions.ThrowIf(Type == 'a');
        Exceptions.ThrowIf(Type == 'c' && Id == 0);
        Exceptions.ThrowIf(Type == 'g' && Id != 0);
    }

    public char Type { get; }

    public long Id { get; }

    public string Name { get; }

    public string Realm { get; }

    public BlizzardRegion Region { get; }

    public string Full { get; }

    //public bool IsValidAccount
    //{
    //    get
    //    {
    //        if (Type != 'a') return false;
    //        if (Name != "null") return false;
    //        if (Realm != "null") return false;
    //        if (IsWildCard) return false;

    //        return true;
    //    }
    //}

    public bool IsValidCharacter
    {
        get
        {
            if (Type != 'c') return false;
            if (Name == null) return false;
            if (Realm == null) return false;
            if (IsWildCard) return false;

            return true;
        }
    }

    public bool IsValidGuild
    {
        get
        {
            if (Type != 'g') return false;
            if (Name == null) return false;
            if (Realm == null) return false;
            if (IsWildCard) return false;

            Exceptions.ThrowIf(Id != 0);

            return true;
        }
    }

    public bool IsWildCard => Id < 0;

    //public static MoaRef GetAccountRef(BlizzardRegion region, long id)
    //{
    //    return new MoaRef('a', region, "null", "null", id);
    //}

    public static MoaRef GetCharacterRef(BlizzardRegion region, string realm, string name, long id)
    {
        return new MoaRef('c', region, realm, name, id);
    }

    public static MoaRef GetGuildRef(BlizzardRegion region, string realm, string name)
    {
        return new MoaRef('g', region, realm, name, 0);
    }

    public string GetLikeQuery()
    {
        return $"{Type}|{Region.ToValue()}|%|{Realm}|{Name}".ToLower();
    }
}