namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryUpdateAuthToken : ICommand<bool>
{
    public string Id { get; init; }

    public string Name { get; init; }

    public string Type { get; init; }

    public string AccessToken { get; init; }

    public string RefreshToken { get; init; }

    public long TokenExpiresAt { get;  init;}
}