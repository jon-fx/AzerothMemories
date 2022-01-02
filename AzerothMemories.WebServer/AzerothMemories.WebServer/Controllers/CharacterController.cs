using AzerothMemories.WebBlazor.Services;

namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class CharacterController : ControllerBase, ICharacterServices
{
    private readonly ICharacterServices _characterServices;
    private readonly CommonServices _commonServices;

    public CharacterController(ICharacterServices characterServices, CommonServices commonServices)
    {
        _characterServices = characterServices;
        _commonServices = commonServices;
    }

    [HttpPost("{characterId}/{newValue}")]
    public Task<bool> TryChangeCharacterAccountSync(Session session, [FromRoute] long characterId, [FromRoute] bool newValue, CancellationToken cancellationToken = default)
    {
        return _characterServices.TryChangeCharacterAccountSync(session, characterId, newValue, cancellationToken);
    }
}