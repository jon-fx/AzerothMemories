namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Account_TryUpdateAuthToken(string Id, string Name, string Type, int? AccountId, string AccessToken, string RefreshToken, long TokenExpiresAt) : ICommand<bool>;