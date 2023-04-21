namespace AzerothMemories.Database.Seeder.Base;

internal abstract class GenericBase<TType> : AbstractBase
{
    protected GenericBase(ILogger<TType> logger, HttpClientProvider clientProvider, WowTools wowTools, MoaResourceWriter resourceWriter) : base(clientProvider, wowTools, resourceWriter)
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