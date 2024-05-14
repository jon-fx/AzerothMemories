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
        AdminServices = _serviceProvider.GetRequiredService<IAdminServices>();
        AccountServices = _serviceProvider.GetRequiredService<IAccountServices>();
        CharacterServices = _serviceProvider.GetRequiredService<ICharacterServices>();
        GuildServices = _serviceProvider.GetRequiredService<IGuildServices>();
        TagServices = _serviceProvider.GetRequiredService<ITagServices>();
        PostServices = _serviceProvider.GetRequiredService<IPostServices>();
        SearchServices = _serviceProvider.GetRequiredService<ISearchServices>();
    }

    public IAdminServices AdminServices { get; private set; } = null!;

    public IAccountServices AccountServices { get; private set; } = null!;

    public ICharacterServices CharacterServices { get; private set; } = null!;

    public IGuildServices GuildServices { get; private set; } = null!;

    public ITagServices TagServices { get; private set; } = null!;

    public IPostServices PostServices { get; private set; } = null!;

    public ISearchServices SearchServices { get; private set; } = null!;
}