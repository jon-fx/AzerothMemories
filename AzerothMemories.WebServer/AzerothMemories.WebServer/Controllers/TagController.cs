namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[Route("api/[controller]/[action]")]
public sealed class TagController : ControllerBase, ITagServices
{
    private readonly CommonServices _commonServices;

    public TagController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet("{searchString}")]
    public Task<PostTagInfo[]> Search(Session session, [FromRoute] string searchString, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.TagServices.Search(session, searchString, locale);
    }
}