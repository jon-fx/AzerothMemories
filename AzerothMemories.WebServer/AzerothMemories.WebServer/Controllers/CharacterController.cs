namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class CharacterController : ControllerBase, ICharacterServices
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
}