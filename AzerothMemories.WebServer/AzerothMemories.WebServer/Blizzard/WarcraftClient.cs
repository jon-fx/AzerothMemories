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

    public Task<RequestResult<AccountProfileSummary>> GetAccountProfile(BlizzardRealmVersion realmVersion, string accessToken, Instant lastModified)
    {
        return Get<AccountProfileSummary>(realmVersion.GetProfileNamespace(), "/profile/user/wow", null, accessToken, false, lastModified);
    }

    public Task<RequestResult<CharacterStatus>> GetCharacterStatusAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName)
    {
        return Get<CharacterStatus>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}/status", null, null, false, null);
    }

    public Task<RequestResult<CharacterProfileSummary>> GetCharacterProfileSummaryAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterProfileSummary>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterAchievementsSummary>> GetCharacterAchievementsSummaryAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterAchievementsSummary>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}/achievements", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterMediaSummary>> GetCharacterRendersAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterMediaSummary>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}/character-media", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterMountsCollectionSummary>> GetCharacterMountsSummaryAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterMountsCollectionSummary>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}/collections/mounts", null, null, false, lastModified);
    }

    public Task<RequestResult<CharacterAchievementStatistics>> GetCharacterStatisticsSummaryAsync(BlizzardRealmVersion realmVersion, string realmName, string characterName, Instant lastModified)
    {
        return Get<CharacterAchievementStatistics>(realmVersion.GetProfileNamespace(), $"/profile/wow/character/{realmName}/{characterName}/achievements/statistics", null, null, false, lastModified);
    }

    public Task<RequestResult<Guild>> GetGuildProfileSummaryAsync(BlizzardRealmVersion realmVersion, string realmName, string guildName, Instant lastModified)
    {
        return Get<Guild>(realmVersion.GetProfileNamespace(), $"/data/wow/guild/{realmName}/{guildName}", null, null, false, lastModified);
    }

    public Task<RequestResult<GuildAchievements>> GetGuildAchievementsAsync(BlizzardRealmVersion realmVersion, string realmName, string guildName, Instant lastModified)
    {
        return Get<GuildAchievements>(realmVersion.GetProfileNamespace(), $"/data/wow/guild/{realmName}/{guildName}/achievements", null, null, false, lastModified);
    }

    public Task<RequestResult<GuildRoster>> GetGuildRosterAsync(BlizzardRealmVersion realmVersion, string realmName, string guildName, Instant lastModified)
    {
        return Get<GuildRoster>(realmVersion.GetProfileNamespace(), $"/data/wow/guild/{realmName}/{guildName}/roster", null, null, false, lastModified);
    }

    public void Dispose()
    {
        _clientProvider.ReturnClient(this);
    }

    public async Task<RequestResult<T>> Get<T>(string blizzardNamespace, string requestUri, string? extra, string? accessToken, bool readAsString, Instant? lastModified) where T : class
    {
        using var client = _clientProvider.CreateClient();

        extra ??= string.Empty;
        accessToken ??= await _clientProvider.GetAccessToken(client).ConfigureAwait(false);
        requestUri = $"{_regionInfo.Host}{requestUri.ToLower()}?namespace={blizzardNamespace}-{_regionInfo.TwoLettersLower}{extra}";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var lastModifiedNotNull = Instant.FromUnixTimeMilliseconds(0);
        if (lastModified != null)
        {
            lastModifiedNotNull = lastModified.Value;
            client.DefaultRequestHeaders.IfModifiedSince = lastModifiedNotNull.ToDateTimeOffset();
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

                string? resultString = null;
                if (readAsString)
                {
                    resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var headersLastModified = response.Content.Headers.LastModified;
                var resultData = await JsonSerializer.DeserializeAsync<T>(contentStream, JsonHelpers.JsonSerializerOptions).ConfigureAwait(false);
                var requestResult = new RequestResult<T>(response.StatusCode, resultData, headersLastModified?.ToInstant() ?? lastModifiedNotNull, resultString);

                return requestResult;
            }

            return new RequestResult<T>(response.StatusCode, null, lastModifiedNotNull, null);
        }
        catch (TaskCanceledException)
        {
            return new RequestResult<T>(HttpStatusCode.RequestTimeout, null, lastModifiedNotNull, null);
        }
        catch (HttpRequestException)
        {
            return new RequestResult<T>(HttpStatusCode.ServiceUnavailable, null, lastModifiedNotNull, null);
        }
    }
}