using NodaTime.Extensions;
using System.Net.Http.Headers;

namespace AzerothMemories.WebServer.Blizzard;

public sealed class WarcraftClient : IDisposable
{
    private readonly WarcraftClientProviderInternal _clientProvider;

    private readonly BlizzardRegionInfo _regionInfo;

    internal WarcraftClient(WarcraftClientProviderInternal clientProvider, BlizzardRegionInfo regionInfo)
    {
        _clientProvider = clientProvider;
        _regionInfo = regionInfo;
    }

    public Task<RequestResult<AccountProfileSummary>> GetAccountProfile(string accessToken)
    {
        return Get<AccountProfileSummary>(BlizzardNamespace.Profile, "/profile/user/wow", null, accessToken, false, null);
    }

    public Task<RequestResult<CharacterStatus>> GetCharacterStatusAsync(string realmName, string characterName)
    {
        return Get<CharacterStatus>(BlizzardNamespace.Profile, $"/profile/wow/character/{realmName}/{characterName}/status", null, null, false, null);
    }

    public Task<RequestResult<CharacterProfileSummary>> GetCharacterProfileSummaryAsync(string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterProfileSummary>(BlizzardNamespace.Profile, $"/profile/wow/character/{realmName}/{characterName}", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterAchievementsSummary>> GetCharacterAchievementsSummaryAsync(string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterAchievementsSummary>(BlizzardNamespace.Profile, $"/profile/wow/character/{realmName}/{characterName}/achievements", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterMediaSummary>> GetCharacterRendersAsync(string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterMediaSummary>(BlizzardNamespace.Profile, $"/profile/wow/character/{realmName}/{characterName}/character-media", null, null, false, lastModified);
    }

    public Task<RequestResult<Guild>> GetGuildProfileSummaryAsync(string realmName, string guildName, Instant lastModified)
    {
        return Get<Guild>(BlizzardNamespace.Profile, $"/data/wow/guild/{realmName}/{guildName}", null, null, false, lastModified);
    }

    public Task<RequestResult<GuildAchievements>> GetGuildAchievementsAsync(string realmName, string guildName, Instant lastModified)
    {
        return Get<GuildAchievements>(BlizzardNamespace.Profile, $"/data/wow/guild/{realmName}/{guildName}/achievements", null, null, false, lastModified);
    }

    public Task<RequestResult<GuildRoster>> GetGuildRosterAsync(string realmName, string guildName, Instant lastModified)
    {
        return Get<GuildRoster>(BlizzardNamespace.Profile, $"/data/wow/guild/{realmName}/{guildName}/roster", null, null, false, lastModified);
    }

    public void Dispose()
    {
        _clientProvider.ReturnClient(this);
    }

    public async Task<RequestResult<T>> Get<T>(BlizzardNamespace blizzardNamespace, string requestUri, string extra, string accessToken, bool readAsString, Instant? lastModified) where T : class
    {
        using var client = _clientProvider.CreateClient();

        extra ??= string.Empty;
        accessToken ??= await _clientProvider.GetAccessToken(client).ConfigureAwait(false);
        requestUri = $"{_regionInfo.Host}{requestUri.ToLower()}?namespace={blizzardNamespace}-{_regionInfo.TwoLetters}{extra}";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (lastModified != null)
        {
            client.DefaultRequestHeaders.IfModifiedSince = lastModified.Value.ToDateTimeOffset();
        }
        else
        {
            client.DefaultRequestHeaders.IfModifiedSince = null;
        }

        try
        {
            using var response = await client.GetAsync(requestUri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                await using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                string resultString = null;
                if (readAsString)
                {
                    resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var resultData = await JsonSerializer.DeserializeAsync<T>(contentStream, JsonHelpers.JsonSerializerOptions).ConfigureAwait(false);
                var requestResult = new RequestResult<T>(response.StatusCode, resultData, response.Content.Headers.LastModified.GetValueOrDefault().ToInstant(), resultString);

                return requestResult;
            }

            return new RequestResult<T>(response.StatusCode, null, Instant.MinValue, null);
        }
        catch (TaskCanceledException)
        {
            return new RequestResult<T>(HttpStatusCode.RequestTimeout, null, Instant.MinValue, null);
        }
        catch (HttpRequestException)
        {
            return new RequestResult<T>(HttpStatusCode.ServiceUnavailable, null, Instant.MinValue, null);
        }
    }
}