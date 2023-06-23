namespace AzerothMemories.WebServer.Controllers;

[ApiController]
[JsonifyErrors]
[UseDefaultSession]
//[AutoValidateAntiforgeryToken]
[Route("api/[controller]/[action]")]
public sealed class AdminController : ControllerBase, IAdminServices
{
    private readonly CommonServices _commonServices;

    public AdminController(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [HttpGet]
    public Task<AdminCountersViewModel> TryGetUserCounts(Session session)
    {
        return _commonServices.AdminServices.TryGetUserCounts(session);
    }

    [HttpGet]
    public Task<ReportedPostViewModel[]> TryGetReportedPosts(Session session)
    {
        return _commonServices.AdminServices.TryGetReportedPosts(session);
    }

    [HttpGet]
    public Task<ReportedPostCommentsViewModel[]> TryGetReportedComments(Session session)
    {
        return _commonServices.AdminServices.TryGetReportedComments(session);
    }

    [HttpGet]
    public Task<ReportedPostTagsViewModel[]> TryGetReportedTags(Session session)
    {
        return _commonServices.AdminServices.TryGetReportedTags(session);
    }

    [HttpPost]
    public Task<bool> SetPostReportResolved([FromBody] Admin_SetPostReportResolved command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> SetPostCommentReportResolved([FromBody] Admin_SetPostCommentReportResolved command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> SetPostTagReportResolved([FromBody] Admin_SetPostTagReportResolved command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }

    [HttpPost]
    public Task<bool> TryBanUser([FromBody] Admin_TryBanUser command, CancellationToken cancellationToken = default)
    {
        return _commonServices.Commander.Call(command, cancellationToken);
    }
}