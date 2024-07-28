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
        DatabaseHub = _serviceProvider.GetRequiredService<DbHub<AppDbContext>>();
        BlizzardUpdateHandler = _serviceProvider.GetRequiredService<BlizzardUpdateHandler>();
        HttpClientProvider = _serviceProvider.GetRequiredService<HttpClientProvider>();

        AdminServices = _serviceProvider.GetRequiredService<AdminServices>();
        AccountServices = _serviceProvider.GetRequiredService<AccountServices>();
        AccountServicesLocal = _serviceProvider.GetRequiredService<AccountServicesLocal>();
        FollowingServices = _serviceProvider.GetRequiredService<FollowingServices>();
        CharacterServices = _serviceProvider.GetRequiredService<CharacterServices>();
        GuildServices = _serviceProvider.GetRequiredService<GuildServices>();
        TagServices = _serviceProvider.GetRequiredService<TagServices>();
        PostServices = _serviceProvider.GetRequiredService<PostServices>();
        SearchServices = _serviceProvider.GetRequiredService<SearchServices>();
        MediaServices = _serviceProvider.GetRequiredService<MediaServices>();
        MarkdownServices = _serviceProvider.GetRequiredService<MarkdownServices>();
    }

    internal IAuth Auth { get; private set; } = null!;

    internal ICommander Commander { get; private set; } = null!;

    internal CommonConfig Config { get; private set; } = null!;

    internal DbHub<AppDbContext> DatabaseHub { get; private set; } = null!;

    internal HttpClientProvider HttpClientProvider { get; private set; } = null!;

    internal AdminServices AdminServices { get; private set; } = null!;

    internal AccountServices AccountServices { get; private set; } = null!;

    internal AccountServicesLocal AccountServicesLocal { get; private set; } = null!;

    internal FollowingServices FollowingServices { get; private set; } = null!;

    internal CharacterServices CharacterServices { get; private set; } = null!;

    internal GuildServices GuildServices { get; private set; } = null!;

    internal TagServices TagServices { get; private set; } = null!;

    internal PostServices PostServices { get; private set; } = null!;

    internal SearchServices SearchServices { get; private set; } = null!;

    internal MediaServices MediaServices { get; private set; } = null!;

    internal BlizzardUpdateHandler BlizzardUpdateHandler { get; private set; } = null!;

    internal MarkdownServices MarkdownServices { get; private set; } = null!;
}