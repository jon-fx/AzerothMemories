namespace AzerothMemories.WebBlazor.Services;

[BasePath("admin")]
public interface IAdminServices
{
    [ComputeMethod, Get(nameof(TryGetUserCounts))]
    Task<AdminCountersViewModel> TryGetUserCounts(Session session);

    [ComputeMethod, Get(nameof(TryGetReportedPosts))]
    Task<ReportedPostViewModel[]> TryGetReportedPosts(Session session);

    [ComputeMethod, Get(nameof(TryGetReportedComments))]
    Task<ReportedPostCommentsViewModel[]> TryGetReportedComments(Session session);

    [ComputeMethod, Get(nameof(TryGetReportedTags))]
    Task<ReportedPostTagsViewModel[]> TryGetReportedTags(Session session);

    [CommandHandler, Post(nameof(SetPostReportResolved))]
    Task<bool> SetPostReportResolved([Body] Admin_SetPostReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler, Post(nameof(SetPostCommentReportResolved))]
    Task<bool> SetPostCommentReportResolved([Body] Admin_SetPostCommentReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler, Post(nameof(SetPostTagReportResolved))]
    Task<bool> SetPostTagReportResolved([Body] Admin_SetPostTagReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler, Post(nameof(TryBanUser))]
    Task<bool> TryBanUser([Body] Admin_TryBanUser command, CancellationToken cancellationToken = default);
}