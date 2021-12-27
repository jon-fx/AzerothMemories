namespace AzerothMemories.WebServer.Blizzard;

public sealed class WarcraftClientProvider
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly CommonConfig _commonConfig;
    private readonly WarcraftClientProviderInternal[] _internalProviders;

    public WarcraftClientProvider(IHttpClientFactory clientFactory, CommonConfig commonConfig)
    {
        _clientFactory = clientFactory;
        _commonConfig = commonConfig;

        _internalProviders = new WarcraftClientProviderInternal[_commonConfig.BlizzardClientInfo.Length];
        for (var i = 0; i < _commonConfig.BlizzardClientInfo.Length; i++)
        {
            var info = _commonConfig.BlizzardClientInfo[i];
            if (info.HasValue)
            {
                _internalProviders[i] = new WarcraftClientProviderInternal(_clientFactory, (BlizzardRegion)i, info.Value.Id, info.Value.Secret);
            }
        }
    }

    public WarcraftClient Get(BlizzardRegion region)
    {
        return _internalProviders[region.ToValue()].GetClient();
    }
}