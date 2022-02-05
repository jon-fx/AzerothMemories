namespace AzerothMemories.WebBlazor.Services;

public sealed class ComputeServices
{
    private readonly IServiceProvider _serviceProvider;

    public ComputeServices(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        AccountServices = _serviceProvider.GetRequiredService<IAccountServices>();
        CharacterServices = _serviceProvider.GetRequiredService<ICharacterServices>();
        GuildServices = _serviceProvider.GetRequiredService<IGuildServices>();
        TagServices = _serviceProvider.GetRequiredService<ITagServices>();
        PostServices = _serviceProvider.GetRequiredService<IPostServices>();
        SearchServices = _serviceProvider.GetRequiredService<ISearchServices>();
    }

    public IAccountServices AccountServices { get; private set; }

    public ICharacterServices CharacterServices { get; private set; }

    public IGuildServices GuildServices { get; private set; }

    public ITagServices TagServices { get; private set; }

    public IPostServices PostServices { get; private set; }

    public ISearchServices SearchServices { get; private set; }
}