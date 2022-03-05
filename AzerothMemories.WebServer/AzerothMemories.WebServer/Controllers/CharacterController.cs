namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class CharacterController : ControllerBase, ICharacterServices
{
    private readonly CommonServices _commonServices;
    private readonly ISessionResolver _sessionResolver;

    public CharacterController(CommonServices commonServices, ISessionResolver sessionResolver)
    {
        _commonServices = commonServices;
        _sessionResolver = sessionResolver;
    }

    [HttpPost]
    public Task<bool> TryChangeCharacterAccountSync(Character_TryChangeCharacterAccountSync command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.CharacterServices.TryChangeCharacterAccountSync(command, cancellationToken);
    }

    [HttpGet("{characterId}"), Publish]
    public Task<CharacterAccountViewModel> TryGetCharacter(Session session, [FromRoute] int characterId)
    {
        return _commonServices.CharacterServices.TryGetCharacter(session, characterId);
    }

    [HttpGet("{region}/{realmSlug}/{characterName}"), Publish]
    public Task<CharacterAccountViewModel> TryGetCharacter(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string realmSlug, [FromRoute] string characterName)
    {
        return _commonServices.CharacterServices.TryGetCharacter(session, region, realmSlug, characterName);
    }

    [HttpPost("{region}/{realmSlug}/{characterName}")]
    public Task<bool> TryEnqueueUpdate(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string realmSlug, [FromRoute] string characterName)
    {
        return _commonServices.CharacterServices.TryEnqueueUpdate(session, region, realmSlug, characterName);
    }

    [HttpPost]
    public Task<bool> TrySetCharacterDeleted(Character_TrySetCharacterDeleted command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.CharacterServices.TrySetCharacterDeleted(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TrySetCharacterRenamedOrTransferred(Character_TrySetCharacterRenamedOrTransferred command, CancellationToken cancellationToken = default)
    {
        command.UseDefaultSession(_sessionResolver);
        return _commonServices.CharacterServices.TrySetCharacterRenamedOrTransferred(command, cancellationToken);
    }
}