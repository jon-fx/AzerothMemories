namespace AzerothMemories.Database.Seeder.Base;

internal abstract class GenericBase<TType> : AbstractBase
{
    protected GenericBase(ILogger<TType> logger, WarcraftClientProvider clientProvider, MoaResourceCache resourceCache, MoaResourceWriter resourceWriter) : base(clientProvider, resourceCache, resourceWriter)
    {
        Logger = logger;
    }

    protected ILogger<TType> Logger { get; }

    public override async Task Execute()
    {
        await ResourceCache.TryLoadResources();

        await DoSomething();
    }

    protected abstract Task DoSomething();
}