namespace AzerothMemories.WebBlazor.Services;

public interface IMoaServices
{
    IAccountServices AccountServices { get; init; }

    ICharacterServices CharacterServices { get; init; }

    ActiveAccountServices ActiveAccountServices { get; init; }

    IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }
}