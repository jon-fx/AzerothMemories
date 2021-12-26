namespace AzerothMemories.Blizzard
{
    internal sealed class AuthAccessToken
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; init; }
        [JsonPropertyName("token_type")] public string TokenType { get; init; }
        [JsonPropertyName("expires_in")] public long ExpiresIn { get; init; }
        [JsonPropertyName("scope")] public string Scope { get; init; }
    }
}