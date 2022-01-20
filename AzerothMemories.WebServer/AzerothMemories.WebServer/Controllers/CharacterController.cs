namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class CharacterController : ControllerBase, ICharacterServices
{
    private readonly CommonServices _commonServices;

    public CharacterController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpPost("{characterId}/{newValue}")]
    public Task<bool> TryChangeCharacterAccountSync(Session session, [FromRoute] long characterId, [FromRoute] bool newValue)
    {
        return _commonServices.CharacterServices.TryChangeCharacterAccountSync(session, characterId, newValue);
    }

    [HttpGet("{characterId}"), Publish]
    public Task<CharacterAccountViewModel> TryGetCharacter(Session session, [FromRoute] long characterId)
    {
        return _commonServices.CharacterServices.TryGetCharacter(session, characterId);
    }

    [HttpGet("{region}/{realmSlug}/{characterName}"), Publish]
    public Task<CharacterAccountViewModel> TryGetCharacter(Session session, [FromRoute] BlizzardRegion region, [FromRoute] string realmSlug, [FromRoute] string characterName)
    {
        return _commonServices.CharacterServices.TryGetCharacter(session, region, realmSlug, characterName);
    }

    [HttpPost("{characterId}")]
    public Task<bool> TrySetCharacterDeleted(Session session, [FromRoute] long characterId)
    {
        return _commonServices.CharacterServices.TrySetCharacterDeleted(session, characterId);
    }

    [HttpPost("{oldCharacterId}/{newCharacterId}")]
    public Task<bool> TrySetCharacterRenamedOrTransferred(Session session, [FromRoute] long oldCharacterId, [FromRoute] long newCharacterId)
    {
        return _commonServices.CharacterServices.TrySetCharacterRenamedOrTransferred(session, oldCharacterId, newCharacterId);
    }
}