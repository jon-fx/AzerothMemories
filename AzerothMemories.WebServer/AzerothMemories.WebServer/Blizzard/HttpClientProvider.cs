namespace AzerothMemories.WebServer.Blizzard;

public sealed class HttpClientProvider
{
    private readonly WarcraftClientProviderInternal[] _internalProviders;

    public HttpClientProvider(IHttpClientFactory clientFactory, CommonConfig commonConfig)
    {
        _internalProviders = new WarcraftClientProviderInternal[commonConfig.BlizzardClientInfo.Length];
        for (var i = 1; i < commonConfig.BlizzardClientInfo.Length; i++)
        {
            var info = commonConfig.BlizzardClientInfo[i];
            if (info.HasValue)
            {
                _internalProviders[i] = new WarcraftClientProviderInternal(clientFactory, (BlizzardRegion)i, info.Value.Id, info.Value.Secret);
            }
        }
    }

    public WarcraftClient GetWarcraftClient(BlizzardRegion region)
    {
        return _internalProviders[region.ToValue()].GetClient();
    }
}