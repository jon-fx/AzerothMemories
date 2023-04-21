namespace AzerothMemories.Database.Seeder.Base;

internal abstract class AbstractBase
{
    protected AbstractBase(HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter)
    {
        WowTools = wowTools;
        ResourceWriter = resourceWriter;
        HttpClientProvider = clientProvider;
    }

    protected MoaResourceCache ResourceCache => WowTools.Main.ResourceCache;

    protected MoaResourceWriter ResourceWriter { get; }

    protected HttpClientProvider HttpClientProvider { get; }

    protected WowTools WowTools { get; }

    public abstract Task Execute();
}