namespace AzerothMemories.Database.Seeder.Base;

internal abstract class AbstractBase
{
    protected AbstractBase(HttpClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter)
    {
        ResourceCache = resourceCache;
        ResourceWriter = resourceWriter;
        HttpClientProvider = clientProvider;
    }

    protected MoaResourceCache ResourceCache { get; }

    protected MoaResourceWriter ResourceWriter { get; }

    protected HttpClientProvider HttpClientProvider { get; }

    protected WowTools WowTools => ResourceCache.WowTools;

    public abstract Task Execute();
}