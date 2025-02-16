using System.Diagnostics.CodeAnalysis;

namespace AzerothMemories.WebBlazor.Common;

public sealed class MoaRef
{
    private MoaRef(char type, BlizzardRegion region, BlizzardRealmVersion realmVersion, string? realm, string? name, long id)
    {
        Id = id;
        Type = type;
        Name = name?.Replace(' ', '-');
        Realm = realm;
        Region = region;
        RealmVersion = realmVersion;

        Full = $"{Type}|{Region.ToValue()}|{Realm}|{Name}|{Id}|{RealmVersion.ToValue()}".ToLower();
    }

    public MoaRef(string full)
    {
        Full = full.ToLower();

        var split = Full.Split('|');
        if (!byte.TryParse(split[1], out var regionId))
        {
            throw new NotImplementedException();
        }

        if (!long.TryParse(split[4], out var id))
        {
            throw new NotImplementedException();
        }

        if (!byte.TryParse(split[5], out var realmVersion))
        {
        }

        Id = id;
        Type = split[0][0];
        Name = split[3];
        Realm = split[2];
        Region = (BlizzardRegion)regionId;
        RealmVersion = (BlizzardRealmVersion)realmVersion;

        Exceptions.ThrowIf(Type == 'a');
        Exceptions.ThrowIf(Type == 'c' && Id == 0);
        Exceptions.ThrowIf(Type == 'g' && Id != 0);
    }

    public char Type { get; }

    public long Id { get; }

    public string? Name { get; }

    public string? Realm { get; }

    public BlizzardRegion Region { get; }

    public string Full { get; }

    public BlizzardRealmVersion RealmVersion { get; }

    [MemberNotNullWhen(true, nameof(Name), nameof(Realm))]
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

    [MemberNotNullWhen(true, nameof(Name), nameof(Realm))]
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

    public static MoaRef GetCharacterRef(BlizzardRegion region, BlizzardRealmVersion realmVersion, string? realm, string? name, long id)
    {
        return new MoaRef('c', region, realmVersion, realm, name, id);
    }

    public static MoaRef GetGuildRef(BlizzardRegion region, BlizzardRealmVersion realmVersion, string? realm, string? name)
    {
        return new MoaRef('g', region, realmVersion, realm, name, 0);
    }

    public string GetLikeQuery()
    {
        return $"{Type}|{Region.ToValue()}|{Realm}|{Name}|".ToLower();
    }
}