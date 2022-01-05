namespace AzerothMemories.WebServer.Services;

public sealed class CommonServices
{
    private readonly IServiceProvider _serviceProvider;

    public CommonServices(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        Auth = _serviceProvider.GetRequiredService<IAuth>();
        Config = _serviceProvider.GetRequiredService<CommonConfig>();
        Commander = _serviceProvider.GetRequiredService<ICommander>();
        DatabaseProvider = _serviceProvider.GetRequiredService<DatabaseProvider>();
        BlizzardUpdateHandler = _serviceProvider.GetRequiredService<BlizzardUpdateHandler>();
        WarcraftClientProvider = _serviceProvider.GetRequiredService<WarcraftClientProvider>();

        AccountServices = _serviceProvider.GetRequiredService<AccountServices>();
        CharacterServices = _serviceProvider.GetRequiredService<CharacterServices>();
        TagServices = _serviceProvider.GetRequiredService<TagServices>();
        PostServices = _serviceProvider.GetRequiredService<PostServices>();
    }

    internal IAuth Auth { get; private set; }

    internal ICommander Commander { get; private set; }

    internal CommonConfig Config { get; private set; }

    internal DatabaseProvider DatabaseProvider { get; private set; }

    internal WarcraftClientProvider WarcraftClientProvider { get; private set; }

    internal AccountServices AccountServices { get; private set; }

    internal CharacterServices CharacterServices { get; private set; }

    internal TagServices TagServices { get; private set; }

    internal PostServices PostServices { get; private set; }

    internal BlizzardUpdateHandler BlizzardUpdateHandler { get; private set; }
}