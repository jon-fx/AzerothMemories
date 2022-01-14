namespace AzerothMemories.WebBlazor.Services;

public interface IMoaServices
{
    IAccountServices AccountServices { get; init; }

    IAccountFollowingServices AccountFollowingServices { get; init; }

    ICharacterServices CharacterServices { get; init; }

    ITagServices TagServices { get; init; }

    IPostServices PostServices { get; init; }

    ISearchServices SearchServices { get; set; }

    ActiveAccountServices ActiveAccountServices { get; init; }

    TagHelpers TagHelpers { get; init; }

    TimeProvider TimeProvider { get; init; }

    DialogHelperService DialogService { get; init; }

    IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }

    NavigationManager NavigationManager { get; init; }
}