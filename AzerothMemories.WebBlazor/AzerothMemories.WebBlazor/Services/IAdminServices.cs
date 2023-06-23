namespace AzerothMemories.WebBlazor.Services;

public interface IAdminServices : IComputeService
{
    [ComputeMethod]
    Task<AdminCountersViewModel> TryGetUserCounts(Session session);

    [ComputeMethod]
    Task<ReportedPostViewModel[]> TryGetReportedPosts(Session session);

    [ComputeMethod]
    Task<ReportedPostCommentsViewModel[]> TryGetReportedComments(Session session);

    [ComputeMethod]
    Task<ReportedPostTagsViewModel[]> TryGetReportedTags(Session session);

    [CommandHandler]
    Task<bool> SetPostReportResolved(Admin_SetPostReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> SetPostCommentReportResolved(Admin_SetPostCommentReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> SetPostTagReportResolved(Admin_SetPostTagReportResolved command, CancellationToken cancellationToken = default);

    [CommandHandler]
    Task<bool> TryBanUser(Admin_TryBanUser command, CancellationToken cancellationToken = default);
}