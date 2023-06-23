namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryUpdateAuthToken : ICommand<bool>
{
    public Account_TryUpdateAuthToken(string id, string name, string type, int? accountId, string accessToken, string refreshToken, long tokenExpiresAt)
    {
        Id = id;
        Name = name;
        Type = type;
        AccountId = accountId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
    }

    [DataMember, MemoryPackInclude] public string Id { get; init; }

    [DataMember, MemoryPackInclude] public string Name { get; init; }

    [DataMember, MemoryPackInclude] public string Type { get; init; }

    [DataMember, MemoryPackInclude] public int? AccountId { get; init; }

    [DataMember, MemoryPackInclude] public string AccessToken { get; init; }

    [DataMember, MemoryPackInclude] public string RefreshToken { get; init; }

    [DataMember, MemoryPackInclude] public long TokenExpiresAt { get; init; }
}