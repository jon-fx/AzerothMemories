namespace AzerothMemories.WebBlazor.Pages;

public sealed class AdminPageViewModel : ViewModelBase
{
    public AdminCountersViewModel Counters { get; private set; }

    public ReportedPostViewModel[] ReportedPosts { get; private set; }

    public ReportedPostTagsViewModel[] ReportedTags { get; private set; }

    public ReportedPostCommentsViewModel[] ReportedComments { get; private set; }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        Counters = await Services.ComputeServices.AdminServices.TryGetUserCounts(Session.Default);

        ReportedPosts = await Services.ComputeServices.AdminServices.TryGetReportedPosts(Session.Default);
        ReportedComments = await Services.ComputeServices.AdminServices.TryGetReportedComments(Session.Default);
        ReportedTags = await Services.ComputeServices.AdminServices.TryGetReportedTags(Session.Default);
    }

    public async Task ResolveReportedPost(bool delete, ReportedPostViewModel viewModel)
    {
        await Services.ClientServices.CommandRunner.Run(new Admin_SetPostReportResolved(Session.Default, delete, viewModel.PostViewModel.Id));
    }

    public async Task ResolveReportedComment(bool delete, ReportedPostCommentsViewModel viewModel)
    {
        await Services.ClientServices.CommandRunner.Run(new Admin_SetPostCommentReportResolved(Session.Default, delete, viewModel.CommentViewModel.PostId, viewModel.CommentViewModel.Id));
    }

    public async Task ResolveReportedTag(bool delete, ReportedPostTagsViewModel viewModel, ReportedChildViewModel row)
    {
        await Services.ClientServices.CommandRunner.Run(new Admin_SetPostTagReportResolved(Session.Default, delete, viewModel.PostViewModel.Id, row.ReportedTag.TagString, row.ReportedTagId));
    }
}