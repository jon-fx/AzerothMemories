namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[UseDefaultSession]
//[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class SearchController : ControllerBase, ISearchServices
{
    private readonly CommonServices _commonServices;

    public SearchController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet, Publish]
    public Task<DailyActivityResults> TryGetDailyActivity(Session session, [FromQuery] string timeZoneId, [FromQuery] byte inZoneDay, [FromQuery] byte inZoneMonth, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.SearchServices.TryGetDailyActivity(session, timeZoneId, inZoneDay, inZoneMonth, locale);
    }

    [HttpGet, Publish]
    public Task<DailyActivityResults[]> TryGetDailyActivityFull(Session session, [FromQuery] string timeZoneId, [FromQuery] byte inZoneDay, [FromQuery] byte inZoneMonth, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.SearchServices.TryGetDailyActivityFull(session, timeZoneId, inZoneDay, inZoneMonth, locale);
    }

    [HttpGet, Publish]
    public Task<MainSearchResult[]> TrySearch(Session session, [FromQuery] MainSearchType searchType, [FromQuery] string searchString)
    {
        return _commonServices.SearchServices.TrySearch(session, searchType, searchString);
    }

    [HttpGet, Publish]
    public Task<RecentPostsResults> TryGetRecentPosts(Session session, [FromQuery] RecentPostsType postsType, [FromQuery] PostSortMode sortMode, [FromQuery] int currentPage, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.SearchServices.TryGetRecentPosts(session, postsType, sortMode, currentPage, locale);
    }

    [HttpGet, Publish]
    public Task<SearchPostsResults> TrySearchPosts(Session session, [FromQuery] string[] tagStrings, [FromQuery] PostSortMode sortMode, [FromQuery] int currentPage, [FromQuery] long postMinTime, [FromQuery] long postMaxTime, [FromQuery] ServerSideLocale locale)
    {
        return _commonServices.SearchServices.TrySearchPosts(session, tagStrings, sortMode, currentPage, postMinTime, postMaxTime, locale);
    }
}