using SixLabors.ImageSharp;
using System.Security.Cryptography;
using System.Text;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IPostServices))]
public class PostServices : IPostServices
{
    private readonly CommonServices _commonServices;

    public PostServices(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    protected virtual async Task<bool> CanAccountSeePost(long activeAccountId, PostRecord postRecord)
    {
        Exceptions.ThrowIf(postRecord == null);

        if (postRecord.PostVisibility == 0)
        {
            return true;
        }

        if (postRecord.AccountId == activeAccountId)
        {
            return true;
        }

        if (activeAccountId > 0)
        {
            var accountRecord = await _commonServices.AccountServices.TryGetAccountRecord(activeAccountId);
            if (accountRecord == null)
            {
                return false;
            }

            if (accountRecord.AccountType >= AccountType.Admin)
            {
                return true;
            }
        }
        else
        {
            return false;
        }

        var following = await _commonServices.FollowingServices.TryGetAccountFollowing(activeAccountId);
        if (following == null || following.Count == 0)
        {
            return false;
        }

        if (!following.TryGetValue(postRecord.AccountId, out var viewModel))
        {
            return false;
        }

        return viewModel.Status == AccountFollowingStatus.Active;
    }

    public async Task<AddMemoryResult> TryPostMemory(Session session, AddMemoryTransferData transferData)
    {
        if (transferData.Comment.Length >= ZExtensions.MaxPostCommentLength)
        {
            return new AddMemoryResult(AddMemoryResultCode.CommentTooLong);
        }

        var dateTime = Instant.FromUnixTimeMilliseconds(transferData.TimeStamp);
        if (dateTime < ZExtensions.MinPostTime || dateTime > SystemClock.Instance.GetCurrentInstant())
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTime);
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionNotFound);
        }

        if (!activeAccount.CanInteract)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionCanNotInteract);
        }

        if (!_commonServices.TagServices.GetCommentText(transferData.Comment, activeAccount.GetUserTagList(), out var commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return new AddMemoryResult(AddMemoryResultCode.ParseCommentFailed);
        }

        var tagRecords = new HashSet<PostTagRecord>();
        var postRecord = new PostRecord
        {
            AccountId = activeAccount.Id,
            PostAvatar = transferData.AvatarTag,
            PostComment = commentText,
            PostTime = dateTime,
            PostCreatedTime = SystemClock.Instance.GetCurrentInstant(),
            PostEditedTime = SystemClock.Instance.GetCurrentInstant(),
            PostVisibility = transferData.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await CreateSystemTags(postRecord, activeAccount, transferData.SystemTags, tagRecords);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagRecords);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        if (!PostTagRecord.ValidateTagCounts(tagRecords))
        {
            return new AddMemoryResult(AddMemoryResultCode.TooManyTags);
        }

        var uploadAndSortResult = await UploadAndSortImages(postRecord, transferData.UploadResults);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        postRecord.Id = await database.InsertWithInt64IdentityAsync(postRecord);

        foreach (var tagRecord in tagRecords)
        {
            tagRecord.PostId = postRecord.Id;
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestored,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        });

        foreach (var userTag in accountsTaggedInComment)
        {
            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedPost,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            });
        }

        transaction.Complete();

        using var computed = Computed.Invalidate();
        _ = _commonServices.AccountServices.GetPostCount(activeAccount.Id);

        return new AddMemoryResult(AddMemoryResultCode.Success, postRecord.AccountId, postRecord.Id);
    }

    private async Task<AddMemoryResultCode> CreateSystemTags(PostRecord postRecord, AccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<PostTagRecord> tagRecords)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResultCode.InvalidTags;
        }

        foreach (var systemTag in systemTags)
        {
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel, PostTagKind.Post);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecord.TagKind = PostTagKind.Post;
            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> AddCommentTags(PostRecord postRecord, HashSet<long> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<PostTagRecord> tagRecords)
    {
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(PostTagType.Account, accountId, PostTagKind.PostComment);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.PostComment);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> UploadAndSortImages(PostRecord postRecord, List<AddMemoryUploadResult> uploadResults)
    {
        var data = new List<(byte[] Buffer, string Hash, string Name, long TimeStamp, string ImageBlobName)>();
        foreach (var uploadResult in uploadResults)
        {
            if (uploadResult == null)
            {
                return AddMemoryResultCode.UploadFailed;
            }

            if (uploadResult.FileContent == null)
            {
                return AddMemoryResultCode.UploadFailed;
            }

            if (uploadResult.FileContent.Length > 1024 * 1024 * 10)
            {
                return AddMemoryResultCode.UploadFailed;
            }

            try
            {
                using var image = Image.Load(uploadResult.FileContent);
                //image.Mutate(x => x.Resize(new ResizeOptions
                //{
                //    Mode = ResizeMode.Max,
                //    Size = new Size(1920, 1080)
                //}));

                await using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream);

                var buffer = memoryStream.ToArray();
                var hashData = MD5.HashData(buffer);
                var hashString = GetHashString(hashData);

                data.Add((buffer, hashString, uploadResult.FileName, uploadResult.FileTimeStamp, "TODO"));
            }
            catch (Exception)
            {
                return AddMemoryResultCode.UploadFailed;
            }
        }

        var imageNameBuilder = new StringBuilder();
        foreach (var uploadResult in data)
        {
            imageNameBuilder.Append(uploadResult.ImageBlobName);
            imageNameBuilder.Append('|');
        }

        postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

        return AddMemoryResultCode.Success;
    }

    private string GetHashString(byte[] hashData)
    {
        var output = new StringBuilder(hashData.Length);
        foreach (var b in hashData)
        {
            output.Append(b.ToString("X2"));
        }

        return output.ToString();
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postId, string locale)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord);
        var posterAccount = await _commonServices.AccountServices.TryGetAccountById(session, postRecord.AccountId);
        if (posterAccount == null)
        {
            return null;
        }

        var postTagInfos = await GetAllPostTagRecord(postId, locale);
        var reactionRecords = await TryGetPostReactions(postId);

        reactionRecords.TryGetValue(activeAccountId, out var reactionViewModel);

        return postRecord.CreatePostViewModel(posterAccount, canSeePost, reactionViewModel, postTagInfos);
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postAccountId, long postId, string locale)
    {
        var result = await TryGetPostViewModel(session, postId, locale);
        if (result.AccountId != postAccountId)
        {
            return null;
        }

        return result;
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostReactionViewModel>> TryGetPostReactions(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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

        return await query.ToDictionaryAsync(x => x.AccountId, x => x);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostReactionViewModel>> TryGetPostCommentReactions(long commentId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

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

        return await query.ToDictionaryAsync(x => x.AccountId, x => x);
    }

    public async Task<long> TryReactToPost(Session session, long postId, PostReaction newReaction)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var postQuery = database.GetUpdateQuery(postRecord, out _);
        var reactionRecord = await database.PostReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.PostId == postId);
        if (reactionRecord == null)
        {
            if (newReaction == PostReaction.None)
            {
                return 0;
            }

            reactionRecord = new PostReactionRecord
            {
                AccountId = activeAccount.Id,
                PostId = postId,
                Reaction = newReaction,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant()
            };

            reactionRecord.Id = await database.InsertWithInt64IdentityAsync(reactionRecord);
            postQuery = ModifyPostQueryWithReaction(postQuery, newReaction, +1, true);
        }
        else
        {
            if (newReaction == reactionRecord.Reaction)
            {
                return reactionRecord.Id;
            }

            var reactionQuery = database.GetUpdateQuery(reactionRecord, out _);
            var previousReaction = reactionRecord.Reaction;
            if (previousReaction != PostReaction.None)
            {
                reactionQuery = reactionQuery.Set(x => x.Reaction, PostReaction.None);
                postQuery = ModifyPostQueryWithReaction(postQuery, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionQuery = reactionQuery.Set(x => x.Reaction, newReaction);
                postQuery = ModifyPostQueryWithReaction(postQuery, newReaction, +1, previousReaction == PostReaction.None);
            }

            await reactionQuery.Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();
        }

        await postQuery.UpdateAsync();

        if (newReaction != PostReaction.None)
        {
            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = activeAccount.Id,
                OtherAccountId = postRecord.AccountId,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToPost1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            });

            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToPost2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            });
        }

        transaction.Complete();

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);
        _ = TryGetPostReactions(postId);
        _ = _commonServices.AccountServices.GetReactionCount(activeAccount.Id);

        return reactionRecord.Id;
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetReactions(Session session, long postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord);
        if (!canSeePost)
        {
            return null;
        }

        var dict = await TryGetPostReactions(postId);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, long postId, int page, long focusedCommentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord);
        if (!canSeePost)
        {
            return null;
        }

        return await TryGetPostCommentsByPage(postId, page, focusedCommentId);
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, long postId, long commentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord);
        if (!canSeePost)
        {
            return null;
        }

        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return null;
        }

        var dict = await TryGetPostCommentReactions(commentId);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<PostCommentPageViewModel> TryGetPostCommentsByPage(long postId, int page, long focusedCommentId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var allCommentPages = await TryGetAllPostComments(postId);
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
    protected virtual async Task<PostCommentPageViewModel[]> TryGetAllPostComments(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from c in database.PostComments
                    from a in database.Accounts.Where(r => r.Id == c.AccountId)
                    where c.PostId == postId
                    orderby c.CreatedTime
                    select c.CreateCommentViewModel(a.Username, a.Avatar);

        var rootCommentNodes = new List<PostCommentViewModel>();
        var allCommentNodes = await query.ToDictionaryAsync(x => x.Id, x => x);

        var allPages = new PostCommentPageViewModel[1];
        allPages[0] = new PostCommentPageViewModel();

        foreach (var kvp in allCommentNodes)
        {
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
    public virtual async Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, long postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return new Dictionary<long, PostCommentReactionViewModel>();
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord);
        if (!canSeePost)
        {
            return new Dictionary<long, PostCommentReactionViewModel>();
        }

        return await TryGetMyCommentReactions(activeAccountId, postId);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(long activeAccountId, long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from reaction in database.PostCommentReactions
                    from comment in database.PostComments.Where(pr => pr.Id == reaction.CommentId)
                    where reaction.AccountId == activeAccountId && comment.PostId == postId
                    select reaction.CreatePostCommentReactionViewModel(null);

        return await query.ToDictionaryAsync(x => x.CommentId, x => x);
    }

    public async Task<bool> TryRestoreMemory(Session session, long postId, long previousCharacterId, long newCharacterId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return false;
        }

        if (!activeAccount.CanInteract)
        {
            return false;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        long? newAccountTag = newCharacterId > 0 ? activeAccount.Id : null;
        long? newCharacterTag = newCharacterId > 0 ? newCharacterId : null;

        long? accountTagToRemove = newCharacterId > 0 ? null : activeAccount.Id;
        long? characterTagToRemove = previousCharacterId > 0 ? previousCharacterId : null;
        if (activeAccount.Id == postRecord.AccountId)
        {
            accountTagToRemove = null;
        }

        if (characterTagToRemove != null && activeAccount.GetCharactersSafe().FirstOrDefault(x => x.Id == characterTagToRemove.Value) == null)
        {
            return false;
        }

        if (newCharacterTag != null && activeAccount.GetCharactersSafe().FirstOrDefault(x => x.Id == newCharacterTag.Value) == null)
        {
            return false;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await TryRestoreMemoryUpdate(database, postId, PostTagType.Account, accountTagToRemove, newAccountTag);
        await TryRestoreMemoryUpdate(database, postId, PostTagType.Character, characterTagToRemove, newCharacterTag);

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal1,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = postRecord.AccountId,
            OtherAccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal2,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        });

        transaction.Complete();

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);
        _ = GetAllPostTags(postId);
        _ = _commonServices.AccountServices.GetMemoryCount(activeAccount.Id);

        return true;
    }

    private async Task TryRestoreMemoryUpdate(DatabaseConnection database, long postId, PostTagType tagType, long? oldTag, long? newTag)
    {
        if (oldTag == null)
        {
        }
        else
        {
            await database.PostTags.Where(x => x.PostId == postId && x.TagType == tagType && x.TagId == oldTag.Value).Set(x => x.TagKind, PostTagKind.Deleted).UpdateAsync();
        }

        if (newTag == null)
        {
        }
        else
        {
            var update = await database.PostTags.Where(x => x.PostId == postId && x.TagType == tagType && x.TagId == newTag.Value).Set(x => x.TagKind, PostTagKind.PostRestored).UpdateAsync();
            if (update == 0)
            {
                var tagString = PostTagInfo.GetTagString(tagType, newTag.Value);

                var record = new PostTagRecord
                {
                    TagId = newTag.Value,
                    TagType = tagType,
                    TagString = tagString,
                    PostId = postId,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    TagKind = PostTagKind.PostRestored
                };

                await database.InsertAsync(record);
            }
        }
    }

    public async Task<long> TryPublishComment(Session session, long postId, long parentCommentId, AddCommentTransferData transferData)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        var commentText = transferData.Comment;
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return 0;
        }

        if (commentText.Length >= 2048)
        {
            return 0;
        }

        PostCommentViewModel parentComment = null;
        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (parentCommentId > 0 && !allComments.TryGetValue(parentCommentId, out parentComment))
        {
            return 0;
        }

        var usersThatCanBeTagged = new Dictionary<long, string>(activeAccount.GetUserTagList());
        if (parentComment != null)
        {
            usersThatCanBeTagged.TryAdd(parentComment.AccountId, parentComment.AccountUsername);
        }

        if (!_commonServices.TagServices.GetCommentText(commentText, usersThatCanBeTagged, out commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return 0;
        }

        var tagRecords = new HashSet<PostTagRecord>();
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(PostTagType.Account, accountId, PostTagKind.UserComment);
            if (tagRecord == null)
            {
                return 0;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.UserComment);
            if (tagRecord == null)
            {
                return 0;
            }

            tagRecords.Add(tagRecord);
        }

        if (tagRecords.Count > 64)
        {
            return 0;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var commentRecord = new PostCommentRecord
        {
            AccountId = activeAccount.Id,
            PostId = postId,
            ParentId = parentComment?.Id,
            PostComment = commentText,
            CreatedTime = SystemClock.Instance.GetCurrentInstant()
        };

        commentRecord.Id = await database.InsertWithInt64IdentityAsync(commentRecord);

        await database.GetUpdateQuery(postRecord, out _).Set(x => x.TotalCommentCount, x => x.TotalCommentCount + 1).UpdateAsync();

        foreach (var tagRecord in tagRecords)
        {
            tagRecord.PostId = postRecord.Id;
            tagRecord.CommentId = commentRecord.Id;
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = activeAccount.Id,
            OtherAccountId = commentRecord.AccountId,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.Commented1,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        });

        await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
        {
            AccountId = commentRecord.AccountId,
            OtherAccountId = activeAccount.Id,
            CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.Commented2,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        });

        foreach (var userTag in accountsTaggedInComment)
        {
            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedComment,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentRecord.Id
            });
        }

        transaction.Complete();

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);
        _ = TryGetAllPostComments(postId);
        _ = _commonServices.AccountServices.GetCommentCount(activeAccount.Id);

        return commentRecord.Id;
    }

    public async Task<long> TryReactToPostComment(Session session, long postId, long commentId, PostReaction newReaction)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var postQuery = database.PostComments.Where(x => x.Id == commentId).AsUpdatable();
        var reactionRecord = await database.PostCommentReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.CommentId == commentId);
        if (reactionRecord == null)
        {
            if (newReaction == PostReaction.None)
            {
                return 0;
            }

            reactionRecord = new PostCommentReactionRecord
            {
                AccountId = activeAccount.Id,
                CommentId = commentId,
                Reaction = newReaction,
                LastUpdateTime = SystemClock.Instance.GetCurrentInstant()
            };

            reactionRecord.Id = await database.InsertWithInt64IdentityAsync(reactionRecord);
            postQuery = ModifyPostQueryWithReaction(postQuery, newReaction, +1, true);
        }
        else
        {
            if (newReaction == reactionRecord.Reaction)
            {
                return reactionRecord.Id;
            }

            var reactionQuery = database.GetUpdateQuery(reactionRecord, out _);
            var previousReaction = reactionRecord.Reaction;
            if (previousReaction != PostReaction.None)
            {
                reactionQuery = reactionQuery.Set(x => x.Reaction, PostReaction.None);
                postQuery = ModifyPostQueryWithReaction(postQuery, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionQuery = reactionQuery.Set(x => x.Reaction, newReaction);
                postQuery = ModifyPostQueryWithReaction(postQuery, newReaction, +1, previousReaction == PostReaction.None);
            }

            await reactionQuery.Set(x => x.LastUpdateTime, SystemClock.Instance.GetCurrentInstant()).UpdateAsync();
        }

        await postQuery.UpdateAsync();

        if (newReaction != PostReaction.None)
        {
            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = activeAccount.Id,
                OtherAccountId = commentViewModel.AccountId,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToComment1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentId
            });

            await _commonServices.AccountServices.TestingHistory(database, new AccountHistoryRecord
            {
                AccountId = commentViewModel.AccountId,
                OtherAccountId = activeAccount.Id,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToComment2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentId
            });
        }

        transaction.Complete();

        using var computed = Computed.Invalidate();
        _ = TryGetAllPostComments(postId);
        _ = TryGetPostCommentReactions(commentId);
        _ = TryGetMyCommentReactions(activeAccount.Id, postId);
        _ = _commonServices.AccountServices.GetReactionCount(activeAccount.Id);

        return reactionRecord.Id;
    }

    public async Task<byte?> TrySetPostVisibility(Session session, long postId, byte newVisibility)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return null;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return null;
        }

        if (activeAccount.Id == postRecord.AccountId)
        {
        }
        else if (activeAccount.AccountType >= AccountType.Admin)
        {
        }
        else
        {
            return null;
        }

        newVisibility = Math.Clamp(newVisibility, (byte)0, (byte)1);

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        await database.GetUpdateQuery(postRecord, out _).Set(x => x.PostVisibility, newVisibility).UpdateAsync();

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);

        return newVisibility;
    }

    public async Task<long> TryDeletePost(Session session, long postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return 0;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();
        if (activeAccount.Id == postRecord.AccountId)
        {
        }
        else if (activeAccount.AccountType >= AccountType.Admin)
        {
            now = -now;
        }
        else
        {
            return 0;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        await database.GetUpdateQuery(postRecord, out _).Set(x => x.DeletedTimeStamp, now).UpdateAsync();

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);

        return now;
    }

    public async Task<long> TryDeleteComment(Session session, long postId, long commentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return 0;
        }

        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds();
        if (activeAccount.Id == commentViewModel.AccountId)
        {
        }
        else if (activeAccount.AccountType >= AccountType.Admin)
        {
            now = -now;
        }
        else
        {
            return 0;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        var query = from comment in database.PostComments
                    where comment.Id == commentId
                    select comment;

        await query.Set(x => x.DeletedTimeStamp, now).UpdateAsync();

        using var computed = Computed.Invalidate();
        _ = TryGetAllPostComments(postId);

        return now;
    }

    public async Task<bool> TryReportPost(Session session, long postId, PostReportInfo reportInfo)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return false;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        var reason = reportInfo.Reason;
        var reasonText = reportInfo.ReasonText;

        if (!Enum.IsDefined(reason))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            return false;
        }

        if (reasonText.Length > ZExtensions.ReportPostCommentMaxLength)
        {
            reasonText = reasonText[..ZExtensions.ReportPostCommentMaxLength];
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var reportQuery = from r in database.PostReports
                          where r.PostId == postRecord.Id && r.AccountId == activeAccount.Id
                          select r;

        var reportQueryResult = await reportQuery.FirstOrDefaultAsync();
        if (reportQueryResult == null)
        {
            var postUpdateQuery = database.GetUpdateQuery(postRecord, out _);

            await postUpdateQuery.Set(x => x.TotalReportCount, x => x.TotalReportCount + 1).UpdateAsync();

            await database.InsertWithInt64IdentityAsync(new PostReportRecord
            {
                AccountId = activeAccount.Id,
                PostId = postRecord.Id,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant()
            });
        }
        else
        {
            var reportUpdateQuery = database.GetUpdateQuery(reportQueryResult, out var reportUpdateQueryChanged);

            if (CheckAndChange.Check(ref reportQueryResult.Reason, reason, ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.Reason, reportQueryResult.Reason);
            }

            if (CheckAndChange.Check(ref reportQueryResult.ReasonText, reasonText, ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.ReasonText, reportQueryResult.ReasonText);
            }

            if (CheckAndChange.Check(ref reportQueryResult.ModifiedTime, SystemClock.Instance.GetCurrentInstant(), ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.ModifiedTime, reportQueryResult.ModifiedTime);
            }

            if (reportUpdateQueryChanged)
            {
                await reportUpdateQuery.UpdateAsync();
            }
        }

        transaction.Complete();

        return true;
    }

    public async Task<bool> TryReportPostComment(Session session, long postId, long commentId, PostReportInfo reportInfo)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return false;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return false;
        }

        var reason = reportInfo.Reason;
        var reasonText = reportInfo.ReasonText;

        if (!Enum.IsDefined(reason))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            return false;
        }

        if (reasonText.Length > ZExtensions.MaxPostCommentLength)
        {
            reasonText = reasonText[..ZExtensions.MaxPostCommentLength];
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var reportQuery = from r in database.PostCommentReports
                          where r.CommentId == commentId && r.AccountId == activeAccount.Id
                          select r;

        var reportQueryResult = await reportQuery.FirstOrDefaultAsync();
        if (reportQueryResult == null)
        {
            var commentUpdateQuery = (from c in database.PostComments
                                      where c.Id == commentId
                                      select c).AsUpdatable();

            commentUpdateQuery = commentUpdateQuery.Set(x => x.TotalReportCount, x => x.TotalReportCount + 1);

            await commentUpdateQuery.UpdateAsync();

            await database.InsertWithInt64IdentityAsync(new PostCommentReportRecord
            {
                AccountId = activeAccount.Id,
                CommentId = commentId,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant(),
            });
        }
        else
        {
            var reportUpdateQuery = database.GetUpdateQuery(reportQueryResult, out var reportUpdateQueryChanged);

            if (CheckAndChange.Check(ref reportQueryResult.Reason, reason, ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.Reason, reportQueryResult.Reason);
            }

            if (CheckAndChange.Check(ref reportQueryResult.ReasonText, reasonText, ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.ReasonText, reportQueryResult.ReasonText);
            }

            if (CheckAndChange.Check(ref reportQueryResult.ModifiedTime, SystemClock.Instance.GetCurrentInstant(), ref reportUpdateQueryChanged))
            {
                reportUpdateQuery = reportUpdateQuery.Set(x => x.ModifiedTime, reportQueryResult.ModifiedTime);
            }

            if (reportUpdateQueryChanged)
            {
                await reportUpdateQuery.UpdateAsync();
            }
        }

        transaction.Complete();

        return true;
    }

    public async Task<bool> TryReportPostTags(Session session, long postId, HashSet<string> tagStrings)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return false;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        var allTagRecords = await GetAllPostTags(postId);
        if (allTagRecords == null || allTagRecords.Length == 0)
        {
            return false;
        }

        var tagRecords = new List<PostTagRecord>();
        foreach (var tagString in tagStrings)
        {
            var tagRecord = allTagRecords.FirstOrDefault(x => x.TagString == tagString);
            if (tagRecord == null)
            {
                return false;
            }

            tagRecords.Add(tagRecord);
        }

        if (tagRecords.Count == 0)
        {
            return false;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        foreach (var tagRecord in tagRecords)
        {
            var reportQuery = from r in database.PostTagReports
                              where r.PostId == postRecord.Id && r.AccountId == activeAccount.Id && r.TagId == tagRecord.Id
                              select r;

            var alreadyReported = await reportQuery.CountAsync() > 0;
            if (alreadyReported)
            {
            }
            else
            {
                var postTagQuery = database.GetUpdateQuery(tagRecord, out _);
                await postTagQuery.Set(x => x.TotalReportCount, x => x.TotalReportCount + 1).UpdateAsync();

                await database.InsertAsync(new PostTagReportRecord
                {
                    AccountId = activeAccount.Id,
                    PostId = postRecord.Id,
                    TagId = tagRecord.Id,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant()
                });
            }
        }

        transaction.Complete();

        //using var computed = Computed.Invalidate();
        //_ = GetPostRecord(postId);
        //_ = GetAllPostTags(postId);

        return true;
    }

    public async Task<AddMemoryResultCode> TryUpdateSystemTags(Session session, long postId, TryUpdateSystemTagsInfo info)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        if (!activeAccount.CanInteract)
        {
            return AddMemoryResultCode.SessionCanNotInteract;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var accountViewModel = activeAccount;
        if (activeAccount.AccountType >= AccountType.Admin)
        {
            accountViewModel = await _commonServices.AccountServices.TryGetAccountById(session, postRecord.AccountId);
        }
        else if (activeAccount.Id != postRecord.AccountId)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        var allTagRecords = await GetAllPostTags(postId);
        var allCurrentTags = allTagRecords.ToDictionary(x => x.TagString, x => x);

        var addedSet = new HashSet<string>(info.NewTags);
        addedSet.ExceptWith(allCurrentTags.Keys);

        var removedSet = new HashSet<string>(allCurrentTags.Keys);
        removedSet.ExceptWith(info.NewTags);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        if (addedSet.Count > 64)
        {
            return AddMemoryResultCode.TooManyTags;
        }

        if (addedSet.Count > 0 || removedSet.Count > 0)
        {
            var recordsToInsert = new HashSet<PostTagRecord>();
            var recordsToUpdate = new HashSet<PostTagRecord>();

            foreach (var systemTag in addedSet)
            {
                var newRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel, PostTagKind.Post);
                if (newRecord == null)
                {
                    return AddMemoryResultCode.InvalidTags;
                }

                var currentTag = await database.PostTags.Where(x => x.PostId == postId && x.TagType == newRecord.TagType && x.TagId == newRecord.TagId).FirstOrDefaultAsync();
                if (currentTag == null)
                {
                    newRecord.PostId = postRecord.Id;
                    newRecord.TagKind = PostTagKind.Post;

                    recordsToInsert.Add(newRecord);
                    allCurrentTags.Add(newRecord.TagString, newRecord);
                }
                else
                {
                    currentTag.TagKind = PostTagKind.Post;

                    recordsToUpdate.Add(currentTag);
                    allCurrentTags.Add(currentTag.TagString, currentTag);
                }
            }

            foreach (var systemTag in removedSet)
            {
                if (allCurrentTags.TryGetValue(systemTag, out var tagRecord))
                {
                    tagRecord.TagKind = PostTagKind.Deleted;
                    recordsToUpdate.Add(tagRecord);
                }
                else
                {
                    return AddMemoryResultCode.InvalidTags;
                }
            }

            if (!PostTagRecord.ValidateTagCounts(allCurrentTags.Values.ToHashSet()))
            {
                return AddMemoryResultCode.TooManyTags;
            }

            if (recordsToInsert.Count > 0)
            {
                await database.PostTags.BulkCopyAsync(recordsToInsert);
            }

            foreach (var tagRecord in recordsToUpdate)
            {
                await database.GetUpdateQuery(tagRecord, out _).Set(x => x.TagKind, tagRecord.TagKind).UpdateAsync();
            }
        }

        var avatar = info.AvatarText;
        if (!string.IsNullOrWhiteSpace(avatar) && !allCurrentTags.ContainsKey(avatar))
        {
            avatar = postRecord.PostAvatar;
        }

        if (!string.IsNullOrWhiteSpace(avatar) && !allCurrentTags.ContainsKey(avatar))
        {
            avatar = null;
        }

        await database.GetUpdateQuery(postRecord, out _).Set(x => x.PostAvatar, avatar).UpdateAsync();

        transaction.Complete();

        InvalidatePostRecordAndTags(postId);

        return AddMemoryResultCode.Success;
    }

    public void InvalidatePostRecordAndTags(long postId)
    {
        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);
        _ = GetAllPostTags(postId);
    }

    [ComputeMethod]
    protected virtual async Task<PostRecord> GetPostRecord(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && p.Id == postId
                    select p;

        return await query.FirstOrDefaultAsync();
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> GetAllPostTagRecord(long postId, string locale)
    {
        var allTagInfo = new List<PostTagInfo>();
        var allTagRecords = await GetAllPostTags(postId);

        foreach (var tagRecord in allTagRecords)
        {
            string hashTagString = null;
            if (tagRecord.TagType == PostTagType.HashTag)
            {
                hashTagString = tagRecord.TagString.Split('-')[1];
            }

            var tagInfo = await _commonServices.TagServices.GetTagInfo(tagRecord.TagType, tagRecord.TagId, hashTagString, locale);
            if (tagInfo == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                allTagInfo.Add(tagInfo);
            }
        }

        return allTagInfo.ToArray();
    }

    [ComputeMethod]
    protected virtual async Task<PostTagRecord[]> GetAllPostTags(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var allTags = from tag in database.PostTags
                      where tag.PostId == postId && (tag.TagKind == PostTagKind.Post || tag.TagKind == PostTagKind.PostComment || tag.TagKind == PostTagKind.PostRestored)
                      select tag;

        return await allTags.ToArrayAsync();
    }

    private static IUpdatable<PostRecord> ModifyPostQueryWithReaction(IUpdatable<PostRecord> query, PostReaction reaction, int change, bool modifyTotal)
    {
        switch (reaction)
        {
            case PostReaction.None:
            {
                return query;
            }
            case PostReaction.Reaction1:
            {
                query = query.Set(x => x.ReactionCount1, x => x.ReactionCount1 + change);
                break;
            }
            case PostReaction.Reaction2:
            {
                query = query.Set(x => x.ReactionCount2, x => x.ReactionCount2 + change);
                break;
            }
            case PostReaction.Reaction3:
            {
                query = query.Set(x => x.ReactionCount3, x => x.ReactionCount3 + change);
                break;
            }
            case PostReaction.Reaction4:
            {
                query = query.Set(x => x.ReactionCount4, x => x.ReactionCount4 + change);
                break;
            }
            case PostReaction.Reaction5:
            {
                query = query.Set(x => x.ReactionCount5, x => x.ReactionCount5 + change);
                break;
            }
            case PostReaction.Reaction6:
            {
                query = query.Set(x => x.ReactionCount6, x => x.ReactionCount6 + change);
                break;
            }
            case PostReaction.Reaction7:
            {
                query = query.Set(x => x.ReactionCount7, x => x.ReactionCount7 + change);
                break;
            }
            case PostReaction.Reaction8:
            {
                query = query.Set(x => x.ReactionCount8, x => x.ReactionCount8 + change);
                break;
            }
            case PostReaction.Reaction9:
            {
                query = query.Set(x => x.ReactionCount9, x => x.ReactionCount9 + change);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (modifyTotal)
        {
            query = query.Set(x => x.TotalReactionCount, x => x.TotalReactionCount + change);
        }

        return query;
    }

    private static IUpdatable<PostCommentRecord> ModifyPostQueryWithReaction(IUpdatable<PostCommentRecord> query, PostReaction reaction, int change, bool modifyTotal)
    {
        switch (reaction)
        {
            case PostReaction.None:
            {
                return query;
            }
            case PostReaction.Reaction1:
            {
                query = query.Set(x => x.ReactionCount1, x => x.ReactionCount1 + change);
                break;
            }
            case PostReaction.Reaction2:
            {
                query = query.Set(x => x.ReactionCount2, x => x.ReactionCount2 + change);
                break;
            }
            case PostReaction.Reaction3:
            {
                query = query.Set(x => x.ReactionCount3, x => x.ReactionCount3 + change);
                break;
            }
            case PostReaction.Reaction4:
            {
                query = query.Set(x => x.ReactionCount4, x => x.ReactionCount4 + change);
                break;
            }
            case PostReaction.Reaction5:
            {
                query = query.Set(x => x.ReactionCount5, x => x.ReactionCount5 + change);
                break;
            }
            case PostReaction.Reaction6:
            {
                query = query.Set(x => x.ReactionCount6, x => x.ReactionCount6 + change);
                break;
            }
            case PostReaction.Reaction7:
            {
                query = query.Set(x => x.ReactionCount7, x => x.ReactionCount7 + change);
                break;
            }
            case PostReaction.Reaction8:
            {
                query = query.Set(x => x.ReactionCount8, x => x.ReactionCount8 + change);
                break;
            }
            case PostReaction.Reaction9:
            {
                query = query.Set(x => x.ReactionCount9, x => x.ReactionCount9 + change);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (modifyTotal)
        {
            query = query.Set(x => x.TotalReactionCount, x => x.TotalReactionCount + change);
        }

        return query;
    }
}