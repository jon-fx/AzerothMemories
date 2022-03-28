using AzerothMemories.WebServer.Services.Handlers;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IAdminServices))]
public class AdminServices : DbServiceBase<AppDbContext>, IAdminServices, IDatabaseContextProvider
{
    private readonly CommonServices _commonServices;

    public AdminServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual async Task<int> GetSessionCount()
    {
        await using var datbase = CreateDbContext();
        return await datbase.Sessions.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod(AutoInvalidateTime = 60)]
    public virtual async Task<int> GetOperationCount()
    {
        await using var datbase = CreateDbContext();
        return await datbase.Operations.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetAccountCount()
    {
        await using var datbase = CreateDbContext();
        return await datbase.Accounts.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetCharacterCount()
    {
        await using var datbase = CreateDbContext();
        return await datbase.Characters.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<int> GetGuildCount()
    {
        await using var datbase = CreateDbContext();
        return await datbase.Guilds.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<int> GetPostCount()
    {
        await _commonServices.PostServices.DependsOnNewPosts().ConfigureAwait(false);

        await using var datbase = CreateDbContext();
        return await datbase.Posts.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<int> GetCommentCount()
    {
        await _commonServices.PostServices.DependsOnNewComments().ConfigureAwait(false);

        await using var datbase = CreateDbContext();
        return await datbase.PostComments.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<int> GetUploadCount()
    {
        await _commonServices.PostServices.DependsOnNewPosts().ConfigureAwait(false);

        await using var datbase = CreateDbContext();
        return await datbase.UploadLogs.CountAsync().ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<AdminCountersViewModel> TryGetUserCounts(Session session)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return null;
        }

        return await TryGetUserCounts().ConfigureAwait(false);
    }

    protected virtual async Task<AdminCountersViewModel> TryGetUserCounts()
    {
        var sessionCount = await GetSessionCount().ConfigureAwait(false);
        var operationCount = await GetOperationCount().ConfigureAwait(false);

        var userCount = await GetAccountCount().ConfigureAwait(false);
        var characterCount = await GetCharacterCount().ConfigureAwait(false);
        var guildCount = await GetGuildCount().ConfigureAwait(false);

        var postCount = await GetPostCount().ConfigureAwait(false);
        var commentCount = await GetCommentCount().ConfigureAwait(false);
        var uploadCount = await GetUploadCount().ConfigureAwait(false);

        return new AdminCountersViewModel
        {
            TimeStamp =  SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds(),
            SessionCount = sessionCount,
            OperationCount = operationCount,

            AcountCount = userCount,
            CharacterCount = characterCount,
            GuildCount = guildCount,

            PostCount = postCount,
            CommentCount = commentCount,
            UploadCount = uploadCount,
        };
    }

    [ComputeMethod]
    public virtual async Task<ReportedPostViewModel[]> TryGetReportedPosts(Session session)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return null;
        }

        var resultViewModels = new List<ReportedPostViewModel>();
        var queryResults = await TryGetReportedPosts().ConfigureAwait(false);
        foreach (var result in queryResults)
        {
            var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(session, result.PostId, ServerSideLocale.En_Gb).ConfigureAwait(false);
            if (postViewModel == null)
            {
                continue;
            }

            var viewModel = new ReportedPostViewModel
            {
                PostViewModel = postViewModel
            };

            foreach (var reportRecord in result.ReportRecords)
            {
                var userTag = await _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, reportRecord.AccountId).ConfigureAwait(false);
                if (userTag == null)
                {
                    continue;
                }

                viewModel.Reports.Add(new ReportedChildViewModel
                {
                    UserTag = userTag,
                    RecordId = reportRecord.Id,
                    Reason = reportRecord.Reason,
                    ReasonText = reportRecord.ReasonText,
                });
            }

            resultViewModels.Add(viewModel);
        }

        return resultViewModels.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<(int PostId, PostReportRecord[] ReportRecords)[]> TryGetReportedPosts()
    {
        await _commonServices.PostServices.DependsOnPostReports().ConfigureAwait(false);

        await using var database = CreateDbContext();

        var query = from report in database.PostReports
                    where report.ResolvedByAccountId == null
                    group report by report.PostId into g
                    select new
                    {
                        PostId = g.Key,
                        ReportCount = g.Count(),
                        ReportRecords = g.ToArray(),
                    };

        var queryResults = await query.OrderBy(x => x.ReportCount).Take(25).ToArrayAsync().ConfigureAwait(false);
        return queryResults.Select(x => (x.PostId, x.ReportRecords)).ToArray();
    }

    [ComputeMethod]
    public virtual async Task<ReportedPostCommentsViewModel[]> TryGetReportedComments(Session session)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return null;
        }

        var resultViewModels = new List<ReportedPostCommentsViewModel>();
        var queryResults = await TryGetReportedComments().ConfigureAwait(false);
        var database = CreateDbContext();

        foreach (var result in queryResults)
        {
            var query = from c in database.PostComments
                        from a in database.Accounts.Where(r => r.Id == c.AccountId)
                        where c.Id == result.CommentId
                        select c.CreateCommentViewModel(a.Username, a.Avatar);

            var commentViewModel = await query.FirstOrDefaultAsync().ConfigureAwait(false);
            if (commentViewModel == null)
            {
                continue;
            }

            var viewModel = new ReportedPostCommentsViewModel
            {
                CommentViewModel = commentViewModel,
            };

            foreach (var reportRecord in result.ReportRecords)
            {
                var userTag = await _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, reportRecord.AccountId).ConfigureAwait(false);
                if (userTag == null)
                {
                    continue;
                }

                viewModel.Reports.Add(new ReportedChildViewModel
                {
                    UserTag = userTag,
                    RecordId = reportRecord.Id,
                    Reason = reportRecord.Reason,
                    ReasonText = reportRecord.ReasonText,
                });
            }

            resultViewModels.Add(viewModel);
        }

        return resultViewModels.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<(int CommentId, PostCommentReportRecord[] ReportRecords)[]> TryGetReportedComments()
    {
        await _commonServices.PostServices.DependsOnPostCommentReports().ConfigureAwait(false);

        await using var database = CreateDbContext();

        var query = from report in database.PostCommentReports
                    where report.ResolvedByAccountId == null
                    group report by report.CommentId into g
                    select new
                    {
                        CommentId = g.Key,
                        ReportCount = g.Count(),
                        ReportRecords = g.ToArray(),
                    };

        var queryResults = await query.OrderBy(x => x.ReportCount).Take(25).ToArrayAsync().ConfigureAwait(false);
        return queryResults.Select(x => (x.CommentId, x.ReportRecords)).ToArray();
    }

    [ComputeMethod]
    public virtual async Task<ReportedPostTagsViewModel[]> TryGetReportedTags(Session session)
    {
        var account = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return null;
        }

        var resultViewModels = new List<ReportedPostTagsViewModel>();
        var queryResults = await TryGetReportedTags().ConfigureAwait(false);
        foreach (var result in queryResults)
        {
            var postViewModel = await _commonServices.PostServices.TryGetPostViewModel(session, result.PostId, ServerSideLocale.En_Gb).ConfigureAwait(false);
            if (postViewModel == null)
            {
                continue;
            }

            var viewModel = new ReportedPostTagsViewModel
            {
                PostViewModel = postViewModel
            };

            foreach (var reportRecord in result.ReportRecords)
            {
                var userTag = await _commonServices.TagServices.TryGetUserTagInfo(PostTagType.Account, reportRecord.AccountId).ConfigureAwait(false);
                if (userTag == null)
                {
                    continue;
                }

                var reportedTag = await _commonServices.TagServices.GetTagInfo(reportRecord.Tag.TagType, reportRecord.Tag.TagId, reportRecord.Tag.TagString, ServerSideLocale.En_Gb).ConfigureAwait(false);
                if (reportedTag == null)
                {
                    continue;
                }

                viewModel.Reports.Add(new ReportedChildViewModel
                {
                    UserTag = userTag,
                    RecordId = reportRecord.Id,
                    ReportedTag = reportedTag,
                    ReportedTagId = reportRecord.TagId,
                });
            }

            resultViewModels.Add(viewModel);
        }

        return resultViewModels.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<(int PostId, PostTagReportRecord[] ReportRecords)[]> TryGetReportedTags()
    {
        await _commonServices.PostServices.DependsOnPostTagReports().ConfigureAwait(false);

        await using var database = CreateDbContext();

        var query2 = from report in database.PostTagReports.Include(x => x.Tag)
                     where report.ResolvedByAccountId == null
                     group report by report.PostId into g
                     select new
                     {
                         PostId = g.Key,
                         ReportCount = g.Count(),
                         ReportRecords = g.ToArray(),
                     };

        var queryResults = await query2.OrderBy(x => x.ReportCount).Take(25).ToArrayAsync().ConfigureAwait(false);
        return queryResults.Select(x => (x.PostId, x.ReportRecords)).ToArray();
    }

    [CommandHandler]
    public virtual async Task<bool> SetPostReportResolved(Admin_SetPostReportResolved command, CancellationToken cancellationToken = default)
    {
        return await AdminServices_SetPostReportResolved.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> SetPostCommentReportResolved(Admin_SetPostCommentReportResolved command, CancellationToken cancellationToken = default)
    {
        return await AdminServices_SetPostCommentReportResolved.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> SetPostTagReportResolved(Admin_SetPostTagReportResolved command, CancellationToken cancellationToken = default)
    {
        return await AdminServices_SetPostTagReportResolved.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryBanUser(Admin_TryBanUser command, CancellationToken cancellationToken = default)
    {
        return await AdminServices_TryBanUser.TryHandle(_commonServices, this, command).ConfigureAwait(false);
    }

    public Task<AppDbContext> CreateCommandDbContext()
    {
        return CreateCommandDbContext(true);
    }
}