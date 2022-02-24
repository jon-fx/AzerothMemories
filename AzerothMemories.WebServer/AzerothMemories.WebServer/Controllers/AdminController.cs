namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
//[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class AdminController : ControllerBase, IAdminServices
{
    private readonly CommonServices _commonServices;
    private readonly ISessionResolver _sessionResolver;

    public AdminController(CommonServices commonServices, ISessionResolver sessionResolver)
    {
        _commonServices = commonServices;
        _sessionResolver = sessionResolver;
    }
}