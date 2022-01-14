namespace AzerothMemories.WebServer.Controllers;

[ApiController, JsonifyErrors]
[Route("api/[controller]/[action]")]
public class SearchPostsController : ControllerBase, ISearchPostsServices
{
    private readonly CommonServices _commonServices;

    public SearchPostsController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet, Publish]
    public Task<RecentPostsResults> TryGetRecentPosts(Session session, [FromQuery] RecentPostsType postsType, [FromQuery] PostSortMode sortMode, [FromQuery] int currentPage, [FromQuery] string locale)
    {
        return _commonServices.SearchPostsServices.TryGetRecentPosts(session, postsType, sortMode, currentPage, locale);
    }

    [HttpGet, Publish]
    public Task<SearchPostsResults> TrySearchPosts(Session session, [FromQuery] string[] tagStrings, [FromQuery] PostSortMode sortMode, [FromQuery] int currentPage, [FromQuery] long postMinTime, [FromQuery] long postMaxTime, [FromQuery] string locale)
    {
        return _commonServices.SearchPostsServices.TrySearchPosts(session, tagStrings, sortMode, currentPage, postMinTime, postMaxTime, locale);
    }
}