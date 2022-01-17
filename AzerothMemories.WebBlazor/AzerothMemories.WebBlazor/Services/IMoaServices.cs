namespace AzerothMemories.WebBlazor.Services;

public interface IMoaServices
{
    IAccountServices AccountServices { get; }

    IAccountFollowingServices AccountFollowingServices { get; }

    ICharacterServices CharacterServices { get; }

    IGuildServices GuildServices { get; }

    ITagServices TagServices { get; }

    IPostServices PostServices { get; }

    ISearchServices SearchServices { get; }

    ActiveAccountServices ActiveAccountServices { get; }

    TagHelpers TagHelpers { get; }

    TimeProvider TimeProvider { get; }

    DialogHelperService DialogService { get; }

    IStringLocalizer<BlizzardResources> StringLocalizer { get; }

    NavigationManager NavigationManager { get; }
}