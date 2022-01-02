namespace AzerothMemories.Database.Seeder.Base;

internal abstract class AbstractBase
{
    protected AbstractBase(WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter)
    {
        ResourceCache = resourceCache;
        ResourceWriter = resourceWriter;
        WarcraftClientProvider = clientProvider;
    }

    protected MoaResourceCache ResourceCache { get; }

    protected MoaResourceWriter ResourceWriter { get; }

    protected WarcraftClientProvider WarcraftClientProvider { get; }

    protected WowTools WowTools => ResourceCache.WowTools;

    public abstract Task Execute();
}