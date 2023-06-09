﻿namespace AzerothMemories.WebServer.Services;

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
        FollowingServices = _serviceProvider.GetRequiredService<FollowingServices>();
        CharacterServices = _serviceProvider.GetRequiredService<CharacterServices>();
        GuildServices = _serviceProvider.GetRequiredService<GuildServices>();
        TagServices = _serviceProvider.GetRequiredService<TagServices>();
        PostServices = _serviceProvider.GetRequiredService<PostServices>();
        SearchServices = _serviceProvider.GetRequiredService<SearchServices>();
        MediaServices = _serviceProvider.GetRequiredService<MediaServices>();
        MarkdownServices = _serviceProvider.GetRequiredService<MarkdownServices>();
    }

    internal IAuth Auth { get; private set; }

    internal ICommander Commander { get; private set; }

    internal CommonConfig Config { get; private set; }

    internal DbHub<AppDbContext> DatabaseHub { get; set; }

    internal HttpClientProvider HttpClientProvider { get; private set; }

    internal AdminServices AdminServices { get; private set; }

    internal AccountServices AccountServices { get; private set; }

    internal FollowingServices FollowingServices { get; private set; }

    internal CharacterServices CharacterServices { get; private set; }

    internal GuildServices GuildServices { get; private set; }

    internal TagServices TagServices { get; private set; }

    internal PostServices PostServices { get; private set; }

    internal SearchServices SearchServices { get; private set; }

    internal MediaServices MediaServices { get; private set; }

    internal BlizzardUpdateHandler BlizzardUpdateHandler { get; private set; }

    internal MarkdownServices MarkdownServices { get; private set; }
}