namespace AzerothMemories.WebServer.Services.Commands;

public sealed record Account_TryUpdateAuthToken : ICommand<bool>
{
    public Account_TryUpdateAuthToken(string id, string? name, string type, int? accountId, string? accessToken, string? refreshToken, long tokenExpiresAt)
    {
        Id = id;
        Name = name;
        Type = type;
        AccountId = accountId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = tokenExpiresAt;
    }

    public string Id { get; init; }

    public string? Name { get; init; }

    public string Type { get; init; }

    public int? AccountId { get; init; }

    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }

    public long TokenExpiresAt { get; init; }
}