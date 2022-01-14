﻿namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class MainSearchResult
{
    [JsonInclude] public long Id { get; init; }
    [JsonInclude] public string RefStr { get; init; }
    [JsonInclude] public string Name { get; init; }

    [JsonInclude] public string Avatar { get; init; }
    [JsonInclude] public byte CharacterClass { get; init; }
    [JsonInclude] public int RealmId { get; init; }

    [JsonInclude] public MainSearchType Type { get; init; }

    public static MainSearchResult CreateAccount(long id, string name, string avatar)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            Type = MainSearchType.Account
        };
    }

    public static MainSearchResult CreateCharacter(long id, string moaRef, string name, string avatar, int realmId, byte characterClass)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            RefStr = moaRef,
            RealmId = realmId,
            CharacterClass = characterClass,
            Type = MainSearchType.Account
        };
    }

    public static MainSearchResult CreateGuild(long id, string moaRef, string name, string avatar, int realmId)
    {
        return new MainSearchResult
        {
            Id = id,
            Name = name,
            Avatar = avatar,
            RefStr = moaRef,
            RealmId = realmId,
            Type = MainSearchType.Account
        };
    }
}