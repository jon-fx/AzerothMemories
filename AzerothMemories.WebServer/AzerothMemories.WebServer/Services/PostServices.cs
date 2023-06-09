﻿namespace AzerothMemories.WebServer.Services;

public class PostServices : IPostServices
{
    private readonly ILogger<PostServices> _logger;
    private readonly CommonServices _commonServices;

    public PostServices(ILogger<PostServices> logger, CommonServices commonServices)
    {
        _logger = logger;
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnNewPosts()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnNewComments()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnPost(int postId)
    {
        return Task.FromResult(postId);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnPostsBy(int accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<string> DependsOnPostsWithTagString(string tagString)
    {
        return Task.FromResult(tagString);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnPostReports()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnPostCommentReports()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnPostTagReports()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual async Task<bool> CanAccountSeePost(int activeAccountId, int postAccountId, byte postVisibility)
    {
        if (postVisibility == 0)
        {
            return true;
        }

        if (postAccountId == activeAccountId)
        {
            return true;
        }

        if (activeAccountId > 0)
        {
            var activeAccountRecord = await _commonServices.AccountServices.TryGetAccountRecord(activeAccountId).ConfigureAwait(false);
            if (activeAccountRecord == null)
            {
                return false;
            }

            if (activeAccountRecord.AccountType >= AccountType.Admin)
            {
                return true;
            }
        }
        else
        {
            return false;
        }

        var following = await _commonServices.FollowingServices.TryGetAccountFollowing(activeAccountId).ConfigureAwait(false);
        if (!following.TryGetValue(postAccountId, out var viewModel))
        {
            return false;
        }

        return viewModel.Status == AccountFollowingStatus.Active;
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(int activeAccountId, int postId, ServerSideLocale locale)
    {
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        var posterAccount = await _commonServices.AccountServices.TryGetAccountRecord(postRecord.AccountId).ConfigureAwait(false);
        if (posterAccount == null)
        {
            return null;
        }

        //if (canSeePost && activeAccountId > 0)
        //{
        //    var timeStampNow = SystemClock.Instance.GetCurrentInstant();
        //    var timeStampNowMs = timeStampNow.ToUnixTimeMilliseconds();
        //    var sessionStamp = await GetSessionPostViewTimeStamp(activeAccountId, postId).ConfigureAwait(false);
        //    if (sessionStamp >= timeStampNowMs)
        //    {
        //        postRecord = await TryUpdatePostViewCount(new Post_UpdateViewCount(activeAccountId, postId)).ConfigureAwait(false);
        //    }
        //}

        var postTagInfos = await GetAllPostTagRecord(postId, locale).ConfigureAwait(false);
        var reactionRecords = await TryGetPostReactions(postId).ConfigureAwait(false);

        reactionRecords.TryGetValue(activeAccountId, out var reactionViewModel);

        return postRecord.CreatePostViewModel(posterAccount, canSeePost, reactionViewModel, postTagInfos);
    }

    [CommandHandler]
    public virtual async Task<AddMemoryResult> TryPostMemory(Post_TryPostMemory command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryPostMemory.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, int postAccountId, int postId, ServerSideLocale locale)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;

        var result = await TryGetPostViewModel(activeAccountId, postId, locale).ConfigureAwait(false);
        if (result == null)
        {
            return null;
        }

        if (result.AccountId != postAccountId)
        {
            return null;
        }

        return result;
    }

    [CommandHandler]
    public virtual async Task<int> TryReactToPost(Post_TryReactToPost command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryReactToPost.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, PostReactionViewModel>> TryGetPostReactions(int postId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var query = from r in database.PostReactions
                    where r.PostId == postId && r.Reaction != PostReaction.None
                    from a in database.Accounts.Where(x => x.Id == r.AccountId)
                    select new PostReactionViewModel
                    {
                        Id = r.Id,
                        Reaction = r.Reaction,
                        AccountId = r.AccountId,
                        AccountUsername = a.Username,
                        AccountAvatar = a.Avatar,
                        LastUpdateTime = r.LastUpdateTime.ToUnixTimeMilliseconds(),
                    };

        return await query.ToDictionaryAsync(x => x.AccountId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, PostReactionViewModel>> TryGetPostCommentReactions(int commentId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var query = from r in database.PostCommentReactions
                    where r.CommentId == commentId && r.Reaction != PostReaction.None
                    from a in database.Accounts.Where(x => x.Id == r.AccountId)
                    select new PostReactionViewModel
                    {
                        Id = r.Id,
                        Reaction = r.Reaction,
                        AccountId = r.AccountId,
                        AccountUsername = a.Username,
                        AccountAvatar = a.Avatar,
                        LastUpdateTime = r.LastUpdateTime.ToUnixTimeMilliseconds(),
                    };

        return await query.ToDictionaryAsync(x => x.AccountId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetReactions(Session session, int postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
        {
            return null;
        }

        var dict = await TryGetPostReactions(postId).ConfigureAwait(false);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, int postId, int page, int focusedCommentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
        {
            return null;
        }

        return await TryGetPostCommentsByPage(postId, page, focusedCommentId).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, int postId, int commentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
        {
            return null;
        }

        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return null;
        }

        var dict = await TryGetPostCommentReactions(commentId).ConfigureAwait(false);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<PostCommentPageViewModel> TryGetPostCommentsByPage(int postId, int page, int focusedCommentId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
        if (allCommentPages.Length == 1)
        {
            return new PostCommentPageViewModel
            {
                Page = 1,
                TotalPages = 1
            };
        }

        if (page == 0 && focusedCommentId > 0 && allCommentPages[0].AllComments.TryGetValue(focusedCommentId, out var focusedComment))
        {
            page = focusedComment.CommentPage;
        }

        page = Math.Clamp(page, 1, allCommentPages.Length);
        return allCommentPages[page];
    }

    [ComputeMethod]
    public virtual async Task<PostCommentPageViewModel[]> TryGetAllPostComments(int postId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var query = from c in database.PostComments
                    from a in database.Accounts.Where(r => r.Id == c.AccountId)
                    where c.PostId == postId
                    orderby c.CreatedTime
                    select c.CreateCommentViewModel(a.Username, a.Avatar);

        var rootCommentNodes = new List<PostCommentViewModel>();
        var allCommentNodes = await query.ToDictionaryAsync(x => x.Id, x => x).ConfigureAwait(false);

        var allPages = new PostCommentPageViewModel[1];
        allPages[0] = new PostCommentPageViewModel();

        foreach (var kvp in allCommentNodes)
        {
            await _commonServices.AccountServices.DependsOnAccountUsername(kvp.Value.AccountId).ConfigureAwait(false);
            await _commonServices.AccountServices.DependsOnAccountAvatar(kvp.Value.AccountId).ConfigureAwait(false);

            if (kvp.Value.ParentId == 0)
            {
                kvp.Value.CommentPage = rootCommentNodes.Count / CommonConfig.CommentsPerPage + 1;

                rootCommentNodes.Add(kvp.Value);

                AddToPage(kvp.Value, ref allPages);
            }
            else
            {
                if (!allCommentNodes.TryGetValue(kvp.Value.ParentId, out var parentTreeItem))
                {
                    throw new NotImplementedException();
                }

                parentTreeItem.Children.Add(kvp.Value);
                kvp.Value.CommentPage = parentTreeItem.CommentPage;

                AddToPage(kvp.Value, ref allPages);
            }
        }

        return allPages.ToArray();
    }

    private void AddToPage(PostCommentViewModel comment, ref PostCommentPageViewModel[] allPages)
    {
        Exceptions.ThrowIf(comment.CommentPage == 0);

        var pageId = comment.CommentPage;

        if (allPages.Length <= pageId)
        {
            Array.Resize(ref allPages, pageId + 1);

            allPages[pageId] = new PostCommentPageViewModel
            {
                Page = pageId
            };

            foreach (var pageModel in allPages)
            {
                pageModel.TotalPages = allPages.Length - 1;
            }
        }

        allPages[0].AllComments.Add(comment.Id, comment);
        allPages[pageId].AllComments.Add(comment.Id, comment);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, int postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return new Dictionary<int, PostCommentReactionViewModel>();
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord.AccountId, postRecord.PostVisibility).ConfigureAwait(false);
        if (!canSeePost)
        {
            return new Dictionary<int, PostCommentReactionViewModel>();
        }

        return await TryGetMyCommentReactions(activeAccountId, postId).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryRestoreMemory(Post_TryRestoreMemory command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryRestoreMemory.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<int> TryPublishComment(Post_TryPublishComment command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryPublishComment.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<int> TryReactToPostComment(Post_TryReactToPostComment command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryReactToPostComment.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<byte?> TrySetPostVisibility(Post_TrySetPostVisibility command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TrySetPostVisibility.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<long> TryDeletePost(Post_TryDeletePost command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryDeletePost.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<long> TryDeleteComment(Post_TryDeleteComment command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryDeleteComment.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPost(Post_TryReportPost command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryReportPost.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPostComment(Post_TryReportPostComment command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryReportPostComment.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPostTags(Post_TryReportPostTags command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryReportPostTags.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [CommandHandler]
    public virtual async Task<AddMemoryResultCode> TryUpdateSystemTags(Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default)
    {
        return await PostServices_TryUpdateSystemTags.TryHandle(_logger, _commonServices, command, cancellationToken).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<Dictionary<int, PostCommentReactionViewModel>> TryGetMyCommentReactions(int activeAccountId, int postId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var query = from reaction in database.PostCommentReactions
                    from comment in database.PostComments.Where(pr => pr.Id == reaction.CommentId)
                    where reaction.AccountId == activeAccountId && comment.PostId == postId
                    select reaction.CreatePostCommentReactionViewModel(null);

        return await query.ToDictionaryAsync(x => x.CommentId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostRecord> TryGetPostRecord(int postId)
    {
        await DependsOnPost(postId).ConfigureAwait(false);

        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        return await database.Posts.FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> GetAllPostTagRecord(int postId, ServerSideLocale locale)
    {
        var allTagInfo = new List<PostTagInfo>();
        var allTagRecords = await GetAllPostTags(postId).ConfigureAwait(false);

        foreach (var tagRecord in allTagRecords)
        {
            var tagInfo = await _commonServices.TagServices.GetTagInfo(tagRecord.TagType, tagRecord.TagId, tagRecord.TagString, locale).ConfigureAwait(false);
            if (tagInfo == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                allTagInfo.Add(tagInfo);
            }
        }

        return allTagInfo.OrderBy(x => x.Type).ThenBy(x => x.Id).ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostTagRecord[]> GetAllPostTags(int postId)
    {
        await using var database = _commonServices.DatabaseHub.CreateDbContext();

        var query = from tag in database.PostTags
                    where tag.PostId == postId
                    select tag;

        var allTags = await query.ToArrayAsync().ConfigureAwait(false);
        return allTags.Where(tag => tag.TagKind == PostTagKind.Post || tag.TagKind == PostTagKind.PostComment || tag.TagKind == PostTagKind.PostRestored).ToArray();
    }
}