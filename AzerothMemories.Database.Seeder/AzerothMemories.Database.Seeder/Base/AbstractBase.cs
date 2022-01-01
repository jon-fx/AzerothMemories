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

    //protected void AddLocalizationData(string key, Name data)
    //{
    //    ResourceWriter.AddLocalizationData(key, data);
    //}

    //protected void AddLocalizationData(string key, Name data, Func<BlizzardLocale, string, string> func)
    //{
    //    ResourceWriter.AddLocalizationData(key, data, func);
    //}

    //protected void AddCommonLocalizationData(string key, string data)
    //{
    //    ResourceWriter.AddCommonLocalizationData(key, data);
    //}

    //protected bool GetLocalizationData(BlizzardLocale locale, string key, out string value)
    //{
    //    return ResourceWriter.GetLocalizationData(locale, key, out value);
    //}
}