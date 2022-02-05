using Azure.Storage.Blobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text;

namespace AzerothMemories.WebServer.Services;

[RegisterComputeService]
[RegisterAlias(typeof(IPostServices))]
public class PostServices : DbServiceBase<AppDbContext>, IPostServices
{
    private readonly CommonServices _commonServices;

    public PostServices(IServiceProvider services, CommonServices commonServices) : base(services)
    {
        _commonServices = commonServices;
    }

    [ComputeMethod]
    public virtual Task<int> DependsOnNewPosts()
    {
        return Task.FromResult(0);
    }

    [ComputeMethod]
    public virtual Task<long> DependsOnPostsBy(long accountId)
    {
        return Task.FromResult(accountId);
    }

    [ComputeMethod]
    public virtual Task<string> DependsOnPostsWithTagString(string tagString)
    {
        return Task.FromResult(tagString);
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

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postId, string locale)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId);
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

    public async Task<string[]> TryUploadScreenShots(Session session, byte[] buffer)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        if (activeAccount == null)
        {
            return Array.Empty<string>();
        }

        await using var stream = new MemoryStream(buffer);
        using var binaryReader = new BinaryReader(stream);

        var count = binaryReader.ReadInt32();
        if (count > 10)
        {
            return Array.Empty<string>();
        }

        var results = new string[count];
        for (var i = 0; i < results.Length; i++)
        {
            try
            {
                var bufferCount = binaryReader.ReadInt32();
                if (bufferCount == -1)
                {
                    continue;
                }

                if (bufferCount == 0 || bufferCount > 1024 * 1024 * 10)
                {
                    break;
                }

                var imageBuffer = binaryReader.ReadBytes(bufferCount);
                using var image = Image.Load(imageBuffer);
                image.Metadata.ExifProfile = null;

                var encoder = new JpegEncoder
                {
                    Quality = 80,
                };

                if (activeAccount.AccountType >= AccountType.Tier1)
                {
                }

                if (activeAccount.AccountType >= AccountType.Tier2)
                {
                    encoder.Quality = 85;
                }

                if (activeAccount.AccountType >= AccountType.Tier3)
                {
                    encoder.Quality = 90;
                }

                await using var memoryStream = new MemoryStream();
                await image.SaveAsJpegAsync(memoryStream, encoder);
                memoryStream.Position = 0;

                var blobName = $"{activeAccount.Id}-{Guid.NewGuid()}.jpg";
                var blobClient = new BlobClient(_commonServices.Config.BlobStorageConnectionString, "moaimages", blobName);
                var result = await blobClient.UploadAsync(memoryStream);
                if (result.Value == null)
                {
                    return null;
                }

                //var buffer = memoryStream.ToArray();
                ////var hashData = MD5.HashData(buffer);
                //var hashString = GetHashString(hashData);

                results[i] = blobName;
            }
            catch (Exception)
            {
                break;
            }
        }

        return results;
    }

    [CommandHandler]
    public virtual async Task<AddMemoryResult> TryPostMemory(Post_TryPostMemory command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = _commonServices.AccountServices.GetPostCount(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = DependsOnPostsBy(invAccount.AccountId);
                _ = _commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = DependsOnPostsWithTagString(tagString);
                }
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = DependsOnNewPosts();
            }

            return default;
        }

        if (command.Comment.Length >= ZExtensions.MaxPostCommentLength)
        {
            return new AddMemoryResult(AddMemoryResultCode.CommentTooLong);
        }

        var dateTime = Instant.FromUnixTimeMilliseconds(command.TimeStamp);
        if (dateTime < ZExtensions.MinPostTime || dateTime > SystemClock.Instance.GetCurrentInstant())
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTime);
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionNotFound);
        }

        if (!activeAccount.CanInteract)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionCanNotInteract);
        }

        if (!_commonServices.TagServices.GetCommentText(command.Comment, activeAccount.GetUserTagList(), out var commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return new AddMemoryResult(AddMemoryResultCode.ParseCommentFailed);
        }

        var tagRecords = new HashSet<PostTagRecord>();
        var postRecord = new PostRecord
        {
            AccountId = activeAccount.Id,
            PostAvatar = command.AvatarTag,
            PostComment = commentText,
            PostTime = dateTime,
            PostCreatedTime = SystemClock.Instance.GetCurrentInstant(),
            PostEditedTime = SystemClock.Instance.GetCurrentInstant(),
            PostVisibility = command.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await CreateSystemTags(postRecord, activeAccount, command.SystemTags, tagRecords);
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

        var imageNameBuilder = new StringBuilder();
        foreach (var blobName in command.BlobNames)
        {
            var split = blobName.Split('-');
            if (split.Length == 0)
            {
                return new AddMemoryResult(AddMemoryResultCode.UploadFailed);
            }

            if (!long.TryParse(split[0], out var blobAccountId))
            {
                return new AddMemoryResult(AddMemoryResultCode.UploadFailed);
            }

            if (activeAccount.Id != blobAccountId)
            {
                return new AddMemoryResult(AddMemoryResultCode.UploadFailed);
            }

            imageNameBuilder.Append(blobName);
            imageNameBuilder.Append('|');
        }

        postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

        await using var database = await CreateCommandDbContext(cancellationToken);

        postRecord.PostTags = tagRecords;

        await database.Posts.AddAsync(postRecord, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestored,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken);

        foreach (var userTag in accountsTaggedInComment)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedPost,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken);
        }

        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));
        context.Operation().Items.Set(new Post_InvalidateTags(postRecord.PostTags.Select(x => x.TagString).ToHashSet()));

        if (postRecord.PostVisibility == 0)
        {
            context.Operation().Items.Set(new Post_InvalidateRecentPost(true));
        }

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

    //private string GetHashString(byte[] hashData)
    //{
    //    var output = new StringBuilder(hashData.Length);
    //    foreach (var b in hashData)
    //    {
    //        output.Append(b.ToString("X2"));
    //    }

    //    return output.ToString();
    //}

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

    [CommandHandler]
    public virtual async Task<long> TryReactToPost(Post_TryReactToPost command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetPostRecord(invPost.PostId);
                _ = TryGetPostReactions(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = _commonServices.AccountServices.GetReactionCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.PostId == postId, cancellationToken);
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

            await database.PostReactions.AddAsync(reactionRecord, cancellationToken);
            await ModifyPostWithReaction(database, postRecord.Id, newReaction, +1, true);
        }
        else
        {
            if (newReaction == reactionRecord.Reaction)
            {
                return reactionRecord.Id;
            }

            var previousReaction = reactionRecord.Reaction;
            if (previousReaction != PostReaction.None)
            {
                reactionRecord.Reaction = PostReaction.None;
                await ModifyPostWithReaction(database, postRecord.Id, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;
                await ModifyPostWithReaction(database, postRecord.Id, newReaction, +1, previousReaction == PostReaction.None);
            }

            reactionRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();
        }

        if (newReaction != PostReaction.None)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = activeAccount.Id,
                OtherAccountId = postRecord.AccountId,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToPost1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken);

            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToPost2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken);
        }

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return reactionRecord.Id;
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostReactionViewModel>> TryGetPostReactions(long postId)
    {
        await using var database = CreateDbContext();

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
        await using var database = CreateDbContext();

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

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetReactions(Session session, long postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId);
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
        var postRecord = await TryGetPostRecord(postId);
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
        var postRecord = await TryGetPostRecord(postId);
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
        await using var database = CreateDbContext();

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
        await using var database = CreateDbContext();

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
        var postRecord = await TryGetPostRecord(postId);
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

    [CommandHandler]
    public virtual async Task<bool> TryRestoreMemory(Post_TryRestoreMemory command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = GetAllPostTags(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = _commonServices.AccountServices.GetMemoryCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        if (!activeAccount.CanInteract)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        long? newAccountTag = command.NewCharacterId > 0 ? activeAccount.Id : null;
        long? newCharacterTag = command.NewCharacterId > 0 ? command.NewCharacterId : null;

        long? accountTagToRemove = command.NewCharacterId > 0 ? null : activeAccount.Id;
        long? characterTagToRemove = command.PreviousCharacterId > 0 ? command.PreviousCharacterId : null;
        var newTagKind = PostTagKind.PostRestored;

        await using var database = await CreateCommandDbContext(cancellationToken);

        if (activeAccount.Id == postRecord.AccountId)
        {
            accountTagToRemove = null;
            newTagKind = PostTagKind.Post;
        }

        if (characterTagToRemove != null && activeAccount.GetCharactersSafe().FirstOrDefault(x => x.Id == characterTagToRemove.Value) == null)
        {
            return false;
        }

        if (newCharacterTag != null && activeAccount.GetCharactersSafe().FirstOrDefault(x => x.Id == newCharacterTag.Value) == null)
        {
            return false;
        }

        await TryRestoreMemoryUpdate(database, postId, PostTagType.Account, accountTagToRemove, newAccountTag, newTagKind);
        await TryRestoreMemoryUpdate(database, postId, PostTagType.Character, characterTagToRemove, newCharacterTag, newTagKind);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal1,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            OtherAccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal2,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken);

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return true;
    }

    private async Task TryRestoreMemoryUpdate(AppDbContext database, long postId, PostTagType tagType, long? oldTag, long? newTag, PostTagKind newTagKind)
    {
        if (oldTag == null)
        {
        }
        else
        {
            await database.PostTags.Where(x => x.PostId == postId && x.TagType == tagType && x.TagId == oldTag.Value).UpdateAsync(r => new PostTagRecord { TagKind = PostTagKind.Deleted });
        }

        if (newTag == null)
        {
        }
        else
        {
            var update = await database.PostTags.Where(x => x.PostId == postId && x.TagType == tagType && x.TagId == newTag.Value).UpdateAsync(r => new PostTagRecord { TagKind = newTagKind });
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
                    TagKind = newTagKind
                };

                await database.PostTags.AddAsync(record);
            }
        }
    }

    [CommandHandler]
    public virtual async Task<long> TryPublishComment(Post_TryPublishComment command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetPostRecord(invPost.PostId);
                _ = TryGetAllPostComments(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = _commonServices.AccountServices.GetCommentCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        var commentText = command.CommentText;
        if (string.IsNullOrWhiteSpace(commentText))
        {
            return 0;
        }

        if (commentText.Length >= 2048)
        {
            return 0;
        }

        PostCommentViewModel parentComment = null;
        var parentCommentId = command.ParentCommentId;
        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (parentCommentId > 0 && !allComments.TryGetValue(parentCommentId, out parentComment))
        {
            return 0;
        }

        {
            var depth = -1;
            var parentId = command.ParentCommentId;
            while (parentId > 0 && allComments.TryGetValue(parentId, out var temp))
            {
                depth++;
                parentId = temp.ParentId;
            }

            if (depth > ZExtensions.MaxCommentDepth)
            {
                return 0;
            }
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

        await using var database = await CreateCommandDbContext(cancellationToken);
        var commentRecord = new PostCommentRecord
        {
            AccountId = activeAccount.Id,
            PostId = postId,
            ParentId = parentComment?.Id,
            PostComment = commentText,
            CreatedTime = SystemClock.Instance.GetCurrentInstant()
        };

        foreach (var tagRecord in tagRecords)
        {
            tagRecord.PostId = postRecord.Id;
            tagRecord.CommentId = commentRecord.Id;
        }

        commentRecord.CommentTags = tagRecords;

        await database.PostComments.AddAsync(commentRecord, cancellationToken);
        await database.Posts.Where(x => x.Id == postRecord.Id).UpdateAsync(r => new PostRecord { TotalCommentCount = r.TotalCommentCount + 1 }, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = commentRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.Commented1,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        }, cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = commentRecord.AccountId,
            OtherAccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.Commented2,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        }, cancellationToken);

        foreach (var userTag in accountsTaggedInComment)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedComment,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentRecord.Id
            }, cancellationToken);
        }

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return commentRecord.Id;
    }

    [CommandHandler]
    public virtual async Task<long> TryReactToPostComment(Post_TryReactToPostComment command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetAllPostComments(invPost.PostId);
                _ = TryGetPostCommentReactions(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = TryGetMyCommentReactions(invAccount.AccountId, invPost?.PostId ?? 0);
                _ = _commonServices.AccountServices.GetReactionCount(invAccount.AccountId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return 0;
        }

        var commentId = command.CommentId;
        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostCommentReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.CommentId == commentId, cancellationToken);
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

            await database.PostCommentReactions.AddAsync(reactionRecord, cancellationToken);
            await ModifyPostCommentWithReaction(database, commentId, newReaction, +1, true);
        }
        else
        {
            if (newReaction == reactionRecord.Reaction)
            {
                return reactionRecord.Id;
            }

            var previousReaction = reactionRecord.Reaction;
            if (previousReaction != PostReaction.None)
            {
                reactionRecord.Reaction = PostReaction.None;
                await ModifyPostCommentWithReaction(database, commentId, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;
                await ModifyPostCommentWithReaction(database, commentId, newReaction, +1, previousReaction == PostReaction.None);
            }

            reactionRecord.LastUpdateTime = SystemClock.Instance.GetCurrentInstant();
        }

        if (newReaction != PostReaction.None)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = activeAccount.Id,
                OtherAccountId = commentViewModel.AccountId,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToComment1,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentId
            }, cancellationToken);

            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = commentViewModel.AccountId,
                OtherAccountId = activeAccount.Id,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.ReactedToComment2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id,
                TargetCommentId = commentId
            }, cancellationToken);
        }

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return reactionRecord.Id;
    }

    [CommandHandler]
    public virtual async Task<byte?> TrySetPostVisibility(Post_TrySetPostVisibility command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetPostRecord(invPost.PostId);
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = DependsOnNewPosts();
            }
            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return null;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
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

        var newVisibility = Math.Clamp(command.NewVisibility, (byte)0, (byte)1);

        await using var database = await CreateCommandDbContext(cancellationToken);
        await database.Posts.Where(x => x.Id == postRecord.Id).UpdateAsync(r => new PostRecord { PostVisibility = newVisibility }, cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateRecentPost(true));

        return newVisibility;
    }

    [CommandHandler]
    public virtual async Task<long> TryDeletePost(Post_TryDeletePost command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetPostRecord(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = DependsOnPostsBy(invAccount.AccountId);
                _ = _commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = DependsOnNewPosts();
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
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

        await using var database = await CreateCommandDbContext(cancellationToken);
        await database.Posts.Where(x => x.Id == postRecord.Id).UpdateAsync(r => new PostRecord { DeletedTimeStamp = now }, cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = activeAccount.Id,
        }, cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(postRecord.AccountId));

        if (postRecord.PostVisibility == 0)
        {
            context.Operation().Items.Set(new Post_InvalidateRecentPost(true));
        }

        return now;
    }

    [CommandHandler]
    public virtual async Task<long> TryDeleteComment(Post_TryDeleteComment command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetAllPostComments(invPost.PostId);
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        var commentId = command.CommentId;
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

        await using var database = await CreateCommandDbContext(cancellationToken);
        await database.PostComments.Where(x => x.Id == commentId).UpdateAsync(r => new PostCommentRecord { DeletedTimeStamp = now }, cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            Type = AccountHistoryType.CommentDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = activeAccount.Id,
        }, cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));

        return now;
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPost(Post_TryReportPost command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        var reason = command.Reason;
        var reasonText = command.ReasonText;

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

        await using var database = await CreateCommandDbContext(cancellationToken);
        var reportQueryResult = await database.PostReports.FirstOrDefaultAsync(r => r.PostId == postRecord.Id && r.AccountId == activeAccount.Id, cancellationToken);
        if (reportQueryResult == null)
        {
            reportQueryResult = new PostReportRecord
            {
                AccountId = activeAccount.Id,
                PostId = postRecord.Id,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant()
            };

            database.PostReports.Add(reportQueryResult);

            await database.Posts.Where(x => x.Id == postId).UpdateAsync(r => new PostRecord { TotalReportCount = r.TotalReportCount + 1 }, cancellationToken);
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
        }

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReported,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = postRecord.AccountId,
        }, cancellationToken);

        await database.SaveChangesAsync(cancellationToken);

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPostComment(Post_TryReportPostComment command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord);
        if (!canSeePost)
        {
            return false;
        }

        var commentId = command.CommentId;
        var allCommentPages = await TryGetAllPostComments(postId);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return false;
        }

        var reason = command.Reason;
        var reasonText = command.ReasonText;

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

        await using var database = await CreateCommandDbContext(cancellationToken);
        var reportQueryResult = await database.PostCommentReports.FirstOrDefaultAsync(r => r.CommentId == commentId && r.AccountId == activeAccount.Id, cancellationToken);
        if (reportQueryResult == null)
        {
            reportQueryResult = new PostCommentReportRecord
            {
                AccountId = activeAccount.Id,
                CommentId = commentId,
                Reason = reason,
                ReasonText = reasonText,
                CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                ModifiedTime = SystemClock.Instance.GetCurrentInstant(),
            };

            database.PostCommentReports.Add(reportQueryResult);

            await database.PostComments.Where(x => x.Id == commentId).UpdateAsync(r => new PostCommentRecord { TotalReportCount = r.TotalReportCount + 1 }, cancellationToken);
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
        }

        await database.SaveChangesAsync(cancellationToken);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedComment,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = commentViewModel.AccountId,
        }, cancellationToken);

        return true;
    }

    [CommandHandler]
    public virtual async Task<bool> TryReportPostTags(Post_TryReportPostTags command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId);
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
        foreach (var tagString in command.TagStrings)
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

        await using var database = await CreateCommandDbContext(cancellationToken);

        var reportQuery = from r in database.PostTagReports
                          where r.PostId == postRecord.Id && r.AccountId == activeAccount.Id
                          select r.TagId;

        var alreadyReported = await reportQuery.ToArrayAsync(cancellationToken);
        var alreadyReportedSet = alreadyReported.ToHashSet();

        foreach (var tagRecord in tagRecords)
        {
            if (alreadyReportedSet.Contains(tagRecord.Id))
            {
            }
            else
            {
                await database.PostTags.Where(x => x.Id == tagRecord.Id).UpdateAsync(r => new PostTagRecord { TotalReportCount = r.TotalReportCount + 1 }, cancellationToken);

                await database.PostTagReports.AddAsync(new PostTagReportRecord
                {
                    AccountId = activeAccount.Id,
                    PostId = postRecord.Id,
                    TagId = tagRecord.Id,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant()
                }, cancellationToken);
            }
        }

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedTags,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = postRecord.AccountId,
        }, cancellationToken);

        await database.SaveChangesAsync(cancellationToken);

        return true;
    }

    [CommandHandler]
    public virtual async Task<AddMemoryResultCode> TryUpdateSystemTags(Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = TryGetPostRecord(invPost.PostId);
                _ = GetAllPostTags(invPost.PostId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = DependsOnPostsWithTagString(tagString);
                }
            }

            return default;
        }

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session);
        if (activeAccount == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        if (!activeAccount.CanInteract)
        {
            return AddMemoryResultCode.SessionCanNotInteract;
        }

        var postId = command.PostId;
        var cachedPostRecord = await TryGetPostRecord(postId);
        if (cachedPostRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var accountViewModel = activeAccount;
        if (activeAccount.AccountType >= AccountType.Admin)
        {
            accountViewModel = await _commonServices.AccountServices.TryGetAccountById(command.Session, cachedPostRecord.AccountId);
        }
        else if (activeAccount.Id != cachedPostRecord.AccountId)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        await using var database = await CreateCommandDbContext(cancellationToken);

        var postRecord = await database.Posts.Include(x => x.PostTags).FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId, cancellationToken);
        if (postRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var allCurrentTags = postRecord.PostTags.Where(x => x.TagKind != PostTagKind.UserComment).ToDictionary(x => x.TagString, x => x);
        var allActiveTags = allCurrentTags.Where(x => x.Value.TagKind != PostTagKind.Deleted).Select(x => x.Key).ToHashSet();

        var addedSet = new HashSet<string>(command.NewTags);
        addedSet.ExceptWith(allActiveTags);

        var removedSet = new HashSet<string>(allActiveTags);
        removedSet.ExceptWith(command.NewTags);

        if (addedSet.Count > 64)
        {
            return AddMemoryResultCode.TooManyTags;
        }

        if (addedSet.Count > 0 || removedSet.Count > 0)
        {
            foreach (var systemTag in addedSet)
            {
                var newRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel, PostTagKind.Post);
                if (newRecord == null)
                {
                    return AddMemoryResultCode.InvalidTags;
                }

                if (allCurrentTags.TryGetValue(newRecord.TagString, out var currentTag))
                {
                    newRecord = currentTag;
                }
                else
                {
                    postRecord.PostTags.Add(newRecord);
                }

                newRecord.PostId = postRecord.Id;
                newRecord.TagKind = PostTagKind.Post;

                allCurrentTags[newRecord.TagString] = newRecord;
            }

            foreach (var systemTag in removedSet)
            {
                if (allCurrentTags.TryGetValue(systemTag, out var tagRecord))
                {
                    tagRecord.TagKind = PostTagKind.Deleted;
                }
                else
                {
                    return AddMemoryResultCode.InvalidTags;
                }
            }

            if (!PostTagRecord.ValidateTagCounts(postRecord.PostTags.ToHashSet()))
            {
                return AddMemoryResultCode.TooManyTags;
            }
        }

        var avatar = command.Avatar;
        var activeTags = postRecord.PostTags.Where(x => x.TagKind != PostTagKind.Deleted).Select(x => x.TagString).ToHashSet();
        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = postRecord.PostAvatar;
        }

        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = null;
        }

        postRecord.PostAvatar = avatar;

        await database.SaveChangesAsync(cancellationToken);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateTags(postRecord.PostTags.Select(x => x.TagString).ToHashSet()));

        return AddMemoryResultCode.Success;
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(long activeAccountId, long postId)
    {
        await using var database = CreateDbContext();

        var query = from reaction in database.PostCommentReactions
                    from comment in database.PostComments.Where(pr => pr.Id == reaction.CommentId)
                    where reaction.AccountId == activeAccountId && comment.PostId == postId
                    select reaction.CreatePostCommentReactionViewModel(null);

        return await query.ToDictionaryAsync(x => x.CommentId, x => x);
    }

    [ComputeMethod]
    public virtual async Task<PostRecord> TryGetPostRecord(long postId)
    {
        await using var database = CreateDbContext();

        return await database.Posts.FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId);
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

        return allTagInfo.OrderBy(x => x.Type).ThenBy(x => x.Id).ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostTagRecord[]> GetAllPostTags(long postId)
    {
        await using var database = CreateDbContext();

        var allTags = from tag in database.PostTags
                      where tag.PostId == postId && (tag.TagKind == PostTagKind.Post || tag.TagKind == PostTagKind.PostComment || tag.TagKind == PostTagKind.PostRestored)
                      select tag;

        return await allTags.ToArrayAsync();
    }

    private static async Task ModifyPostWithReaction(AppDbContext dbContext, long id, PostReaction reaction, int change, bool modifyTotal)
    {
        switch (reaction)
        {
            case PostReaction.None:
                {
                    //return query;
                    break;
                }
            case PostReaction.Reaction1:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount1 = r.ReactionCount1 + change });
                    break;
                }
            case PostReaction.Reaction2:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount2 = r.ReactionCount2 + change });
                    break;
                }
            case PostReaction.Reaction3:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount3 = r.ReactionCount3 + change });
                    break;
                }
            case PostReaction.Reaction4:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount4 = r.ReactionCount4 + change });
                    break;
                }
            case PostReaction.Reaction5:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount5 = r.ReactionCount5 + change });
                    break;
                }
            case PostReaction.Reaction6:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount6 = r.ReactionCount6 + change });
                    break;
                }
            case PostReaction.Reaction7:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount7 = r.ReactionCount7 + change });
                    break;
                }
            case PostReaction.Reaction8:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount8 = r.ReactionCount8 + change });
                    break;
                }
            case PostReaction.Reaction9:
                {
                    await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { ReactionCount9 = r.ReactionCount9 + change });
                    break;
                }
            default:
                {
                    throw new ArgumentOutOfRangeException();
                }
        }

        if (modifyTotal)
        {
            await dbContext.Posts.Where(x => x.Id == id).UpdateAsync(r => new PostRecord { TotalReactionCount = r.TotalReactionCount + change });
        }
    }

    private static async Task ModifyPostCommentWithReaction(AppDbContext dbContext, long id, PostReaction reaction, int change, bool modifyTotal)
    {
        switch (reaction)
        {
            case PostReaction.None:
                {
                    //return query;
                    break;
                }
            case PostReaction.Reaction1:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount1 = r.ReactionCount1 + change });
                    break;
                }
            case PostReaction.Reaction2:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount2 = r.ReactionCount2 + change });
                    break;
                }
            case PostReaction.Reaction3:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount3 = r.ReactionCount3 + change });
                    break;
                }
            case PostReaction.Reaction4:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount4 = r.ReactionCount4 + change });
                    break;
                }
            case PostReaction.Reaction5:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount5 = r.ReactionCount5 + change });
                    break;
                }
            case PostReaction.Reaction6:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount6 = r.ReactionCount6 + change });
                    break;
                }
            case PostReaction.Reaction7:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount7 = r.ReactionCount7 + change });
                    break;
                }
            case PostReaction.Reaction8:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount8 = r.ReactionCount8 + change });
                    break;
                }
            case PostReaction.Reaction9:
                {
                    await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { ReactionCount9 = r.ReactionCount9 + change });
                    break;
                }
            default:
                {
                    throw new ArgumentOutOfRangeException();
                }
        }

        if (modifyTotal)
        {
            await dbContext.PostComments.Where(x => x.Id == id).UpdateAsync(r => new PostCommentRecord { TotalReactionCount = r.TotalReactionCount + change });
        }
    }
}