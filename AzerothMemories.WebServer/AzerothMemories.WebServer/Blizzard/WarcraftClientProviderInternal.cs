using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;

namespace AzerothMemories.WebServer.Blizzard;

internal sealed class WarcraftClientProviderInternal
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly BlizzardRegion _blizzardRegion;
    private readonly BlizzardRegionInfo _blizzardRegionInfo;
    private readonly ConcurrentBag<WarcraftClient> _warcraftClients;

    private readonly string _clientId;
    private readonly string _clientSecret;

    private AuthAccessToken _token;
    private DateTime _tokenExpiration;

    public WarcraftClientProviderInternal(IHttpClientFactory clientFactory, BlizzardRegion blizzardRegion, string clientId, string clientSecret)
    {
        _clientFactory = clientFactory;
        _blizzardRegion = blizzardRegion;
        _blizzardRegionInfo = blizzardRegion.ToInfo();
        _warcraftClients = new ConcurrentBag<WarcraftClient>();

        _clientId = clientId;
        _clientSecret = clientSecret;

        Exceptions.ThrowIf(_blizzardRegionInfo == null);
    }

    public WarcraftClient GetClient()
    {
        if (!_warcraftClients.TryTake(out var client))
        {
            client = new WarcraftClient(this, _blizzardRegionInfo);
        }

        return client;
    }

    public void ReturnClient(WarcraftClient client)
    {
        _warcraftClients.Add(client);
    }

    public HttpClient CreateClient()
    {
        return _clientFactory.CreateClient("Blizzard");
    }

    public async Task<string> GetAccessToken(HttpClient client)
    {
        if (TokenHasExpired)
        {
            _token = await GetOAuthToken(client).ConfigureAwait(false);
            _tokenExpiration = DateTime.UtcNow.AddSeconds(_token.ExpiresIn).AddSeconds(-30);
        }

        return _token.AccessToken;
    }

    public bool TokenHasExpired => _token == null || DateTime.UtcNow >= _tokenExpiration;

    private async Task<AuthAccessToken> GetOAuthToken(HttpClient client)
    {
        var credentials = $"{_clientId}:{_clientSecret}";
        var oauthHost = _blizzardRegionInfo.TokenEndpoint;

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "wow.profile")
        });

        var request = await client.PostAsync(oauthHost, requestBody).ConfigureAwait(false);
        var response = await request.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<AuthAccessToken>(response);
    }
}