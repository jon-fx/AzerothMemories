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
    public Task<bool> TryChangeCharacterAccountSync(Session session, [FromRoute] long characterId, [FromRoute] bool newValue, CancellationToken cancellationToken = default)
    {
        return _commonServices.CharacterServices.TryChangeCharacterAccountSync(session, characterId, newValue, cancellationToken);
    }
}