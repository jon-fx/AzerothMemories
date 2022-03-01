using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Security.Cryptography;
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
            var accountRecord = await _commonServices.AccountServices.TryGetAccountRecord(activeAccountId).ConfigureAwait(false);
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

        var following = await _commonServices.FollowingServices.TryGetAccountFollowing(activeAccountId).ConfigureAwait(false);
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
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postId, ServerSideLocale locale)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord).ConfigureAwait(false);
        var posterAccount = await _commonServices.AccountServices.TryGetAccountById(session, postRecord.AccountId).ConfigureAwait(false);
        if (posterAccount == null)
        {
            return null;
        }

        var postTagInfos = await GetAllPostTagRecord(postId, locale).ConfigureAwait(false);
        var reactionRecords = await TryGetPostReactions(postId).ConfigureAwait(false);

        reactionRecords.TryGetValue(activeAccountId, out var reactionViewModel);

        return postRecord.CreatePostViewModel(posterAccount, canSeePost, reactionViewModel, postTagInfos);
    }

    public Task<AddMemoryResult> TryPostMemory(Session session, byte[] buffer)
    {
        try
        {
            using var memoryStream = new MemoryStream(buffer);
            using var binaryReader = new BinaryReader(memoryStream);

            var timeStamp = binaryReader.ReadInt64();
            var avatarTag = binaryReader.ReadString();
            var isPrivate = binaryReader.ReadBoolean();
            var comment = binaryReader.ReadString();
            var tagCount = binaryReader.ReadInt32();
            var systemTags = new HashSet<string>();
            for (var i = 0; i < tagCount; i++)
            {
                systemTags.Add(binaryReader.ReadString());
            }

            var imageDataCount = binaryReader.ReadInt32();
            var imageData = new List<byte[]>(imageDataCount);
            for (var i = 0; i < imageDataCount; i++)
            {
                var byteCount = binaryReader.ReadInt32();
                var imageBuffer = binaryReader.ReadBytes(byteCount);
                imageData.Add(imageBuffer);
            }

            if (string.IsNullOrWhiteSpace(avatarTag))
            {
                avatarTag = null;
            }

            var command = new Post_TryPostMemory
            {
                Session = session,
                TimeStamp = timeStamp,
                AvatarTag = avatarTag,
                IsPrivate = isPrivate,
                Comment = comment,
                SystemTags = systemTags,
                ImageData = imageData,
            };

            return TryPostMemory(command);
        }
        catch (Exception)
        {
            return Task.FromResult(new AddMemoryResult(AddMemoryResultCode.Failed));
        }
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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
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

        var buildSystemTagsResult = await CreateSystemTags(postRecord, activeAccount, command.SystemTags, tagRecords).ConfigureAwait(false);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagRecords).ConfigureAwait(false);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        if (!PostTagRecord.ValidateTagCounts(tagRecords))
        {
            return new AddMemoryResult(AddMemoryResultCode.TooManyTags);
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var uploadAndSortResult = await UploadAndSortImages(database, activeAccount, postRecord, command.ImageData).ConfigureAwait(false);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        command.ImageData.Clear();

        postRecord.PostTags = tagRecords;

        await database.Posts.AddAsync(postRecord, cancellationToken).ConfigureAwait(false);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestored,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken).ConfigureAwait(false);

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
            }, cancellationToken).ConfigureAwait(false);
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
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
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
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(PostTagType.Account, accountId, PostTagKind.PostComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.PostComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> UploadAndSortImages(AppDbContext database, AccountViewModel accountViewModel, PostRecord postRecord, List<byte[]> imageDataList)
    {
        if (imageDataList.Count > ZExtensions.MaxPostScreenShots)
        {
            return AddMemoryResultCode.UploadFailed;
        }

        var timeAgo = SystemClock.Instance.GetCurrentInstant() - _commonServices.Config.UploadsInTheLastXDuration;
        var uploadsInTheLastX = database.UploadLogs.Count(x => x.AccountId == accountViewModel.Id && x.UploadTime > timeAgo);
        if (uploadsInTheLastX > _commonServices.Config.UploadsInTheLastXCount)
        {
            return AddMemoryResultCode.UploadFailedQuota;
        }

        var dataToUpload = new (string Extension, BinaryData BlobData, string BlobHash)[imageDataList.Count];
        for (var i = 0; i < imageDataList.Count; i++)
        {
            try
            {
                var buffer = imageDataList[i];
                var bufferCount = buffer.Length;
                if (bufferCount == 0 || bufferCount > ZExtensions.MaxAddMemoryFileSizeInBytes)
                {
                    break;
                }

                await using var memoryStream = new MemoryStream();
                using var image = Image.Load(buffer);
                var extension = "jpg";
                image.Metadata.ExifProfile = null;

                if (image.Frames.Count > 1)
                {
                    var encoder = new GifEncoder();

                    await image.SaveAsGifAsync(memoryStream, encoder).ConfigureAwait(false);
                    memoryStream.Position = 0;
                    extension = "gif";
                }
                else
                {
                    var encoder = new JpegEncoder();

                    await image.SaveAsJpegAsync(memoryStream, encoder).ConfigureAwait(false);
                    memoryStream.Position = 0;

                    if (memoryStream.Length > 1.Megabytes().Bytes)
                    {
                        encoder.Quality = accountViewModel.GetUploadQuality();

                        await image.SaveAsJpegAsync(memoryStream, encoder).ConfigureAwait(false);
                        memoryStream.Position = 0;
                    }
                }

                var blobData = memoryStream.ToArray();
                var hashBytes = MD5.HashData(blobData);
                dataToUpload[i] = (extension, new BinaryData(blobData), CreateHashString(hashBytes));
            }
            catch (Exception)
            {
                return AddMemoryResultCode.UploadFailed;
            }
        }

        foreach (var valueTuple in dataToUpload)
        {
            var count = database.UploadLogs.Count(x => x.AccountId == accountViewModel.Id && x.BlobHash == valueTuple.BlobHash);
            if (count > _commonServices.Config.MaxUploadsWithTheSameHash)
            {
                return AddMemoryResultCode.UploadFailedHash;
            }
        }

        try
        {
            var imageNameBuilder = new StringBuilder();
            foreach (var (extension, blobData, blobHash) in dataToUpload)
            {
                var blobName = $"{accountViewModel.Id}-{Guid.NewGuid()}.{extension}";
                if (_commonServices.Config.UploadToBlobStorage)
                {
                    var blobClient = new Azure.Storage.Blobs.BlobClient(_commonServices.Config.BlobStorageConnectionString, ZExtensions.BlobImages, blobName);
                    var result = await blobClient.UploadAsync(blobData).ConfigureAwait(false);
                    if (result.Value == null)
                    {
                        return AddMemoryResultCode.UploadFailed;
                    }

                    var azureHash = CreateHashString(result.Value.ContentHash);
                    if (azureHash != blobHash)
                    {
                        return AddMemoryResultCode.UploadFailedHashCheck;
                    }
                }

                imageNameBuilder.Append(blobName);
                imageNameBuilder.Append('|');

                database.UploadLogs.Add(new AccountUploadLog
                {
                    AccountId = accountViewModel.Id,
                    BlobName = blobName,
                    BlobHash = blobHash,
                    UploadTime = SystemClock.Instance.GetCurrentInstant()
                });
            }

            postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

            return AddMemoryResultCode.Success;
        }
        catch (Exception)
        {
            return AddMemoryResultCode.UploadFailed;
        }
    }

    private string CreateHashString(byte[] hashBytes)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
    }

    [ComputeMethod]
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postAccountId, long postId, ServerSideLocale locale)
    {
        var result = await TryGetPostViewModel(session, postId, locale).ConfigureAwait(false);
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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return 0;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.PostId == postId, cancellationToken).ConfigureAwait(false);
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

            await database.PostReactions.AddAsync(reactionRecord, cancellationToken).ConfigureAwait(false);

            ModifyPostWithReaction(postRecord, newReaction, +1, true);
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
                ModifyPostWithReaction(postRecord, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;
                ModifyPostWithReaction(postRecord, newReaction, +1, previousReaction == PostReaction.None);
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
            }, cancellationToken).ConfigureAwait(false);

            if (activeAccount.Id != postRecord.AccountId)
            {
                await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
                {
                    AccountId = postRecord.AccountId,
                    OtherAccountId = activeAccount.Id,
                    //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    Type = AccountHistoryType.ReactedToPost2,
                    TargetId = postRecord.AccountId,
                    TargetPostId = postRecord.Id
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        return await query.ToDictionaryAsync(x => x.AccountId, x => x).ConfigureAwait(false);
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

        return await query.ToDictionaryAsync(x => x.AccountId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetReactions(Session session, long postId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return null;
        }

        var dict = await TryGetPostReactions(postId).ConfigureAwait(false);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, long postId, int page, long focusedCommentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return null;
        }

        return await TryGetPostCommentsByPage(postId, page, focusedCommentId).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, long postId, long commentId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return null;
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord).ConfigureAwait(false);
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
    protected virtual async Task<PostCommentPageViewModel> TryGetPostCommentsByPage(long postId, int page, long focusedCommentId)
    {
        await using var database = CreateDbContext();

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
    protected virtual async Task<PostCommentPageViewModel[]> TryGetAllPostComments(long postId)
    {
        await using var database = CreateDbContext();

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
        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(session).ConfigureAwait(false);
        var activeAccountId = activeAccount?.Id ?? 0;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return new Dictionary<long, PostCommentReactionViewModel>();
        }

        var canSeePost = await CanAccountSeePost(activeAccountId, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return new Dictionary<long, PostCommentReactionViewModel>();
        }

        return await TryGetMyCommentReactions(activeAccountId, postId).ConfigureAwait(false);
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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        if (!activeAccount.CanInteract)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return false;
        }

        var newTagKind = PostTagKind.PostRestored;
        long? accountTagToAdd = command.NewCharacterId > 0 ? activeAccount.Id : null;
        long? characterTagToAdd = command.NewCharacterId > 0 ? command.NewCharacterId : null;
        long? accountTagToRemove = command.NewCharacterId > 0 ? null : activeAccount.Id;

        if (activeAccount.Id == postRecord.AccountId)
        {
            accountTagToRemove = null;
            newTagKind = PostTagKind.Post;
        }
        else
        {
            var accountRecord = await _commonServices.AccountServices.TryGetAccountRecord(postRecord.AccountId).ConfigureAwait(false);
            if (accountRecord.BlizzardRegionId != activeAccount.RegionId)
            {
                return false;
            }
        }

        var accountCharacters = activeAccount.GetAllCharactersSafe();
        if (characterTagToAdd != null && accountCharacters.FirstOrDefault(x => x.Id == characterTagToAdd.Value) == null)
        {
            return false;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        if (accountTagToRemove != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Account, accountTagToRemove.Value);
            var myAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
            if (myAccountTag == null)
            {
            }
            else if (myAccountTag.TagKind == PostTagKind.Deleted)
            {
            }
            else if (myAccountTag.TagKind == PostTagKind.DeletedByPoster)
            {
            }
            else
            {
                myAccountTag.TagKind = PostTagKind.Deleted;
            }
        }

        if (accountTagToAdd != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Account, accountTagToAdd.Value);
            var newAccountTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
            if (newAccountTag == null)
            {
                newAccountTag = new PostTagRecord
                {
                    TagId = accountTagToAdd.Value,
                    TagType = PostTagType.Account,
                    TagString = tagString,
                    PostId = postId,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    TagKind = newTagKind
                };

                await database.PostTags.AddAsync(newAccountTag, cancellationToken).ConfigureAwait(false);
            }
            else if (newTagKind == PostTagKind.PostRestored && newAccountTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }
            else
            {
                newAccountTag.TagKind = newTagKind;
            }
        }

        var characterTagStrings = accountCharacters.Select(x => x.TagString).ToList();
        var myCharacterTags = await database.PostTags.Where(x => x.PostId == postId && characterTagStrings.Contains(x.TagString)).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var characterTag in myCharacterTags)
        {
            if (newTagKind == PostTagKind.PostRestored && characterTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }

            if (characterTag.TagKind == PostTagKind.Deleted)
            {
                continue;
            }

            characterTag.TagKind = PostTagKind.Deleted;
        }

        if (characterTagToAdd != null)
        {
            var tagString = PostTagInfo.GetTagString(PostTagType.Character, characterTagToAdd.Value);
            var newCharacterTag = await database.PostTags.FirstOrDefaultAsync(x => x.PostId == postId && x.TagString == tagString, cancellationToken).ConfigureAwait(false);
            if (newCharacterTag == null)
            {
                newCharacterTag = new PostTagRecord
                {
                    TagId = characterTagToAdd.Value,
                    TagType = PostTagType.Character,
                    TagString = tagString,
                    PostId = postId,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    TagKind = newTagKind
                };

                await database.PostTags.AddAsync(newCharacterTag, cancellationToken).ConfigureAwait(false);
            }
            else if (newTagKind == PostTagKind.PostRestored && newCharacterTag.TagKind == PostTagKind.DeletedByPoster)
            {
                return false;
            }
            else
            {
                newCharacterTag.TagKind = newTagKind;
            }
        }

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestoredExternal1,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken).ConfigureAwait(false);

        if (activeAccount.Id != postRecord.AccountId)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.MemoryRestoredExternal2,
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Post_InvalidatePost(postId));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));

        return true;
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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
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
        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
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
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(PostTagType.Account, accountId, PostTagKind.UserComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return 0;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.UserComment).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

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

        postRecord.TotalCommentCount++;
        commentRecord.CommentTags = tagRecords;

        await database.PostComments.AddAsync(commentRecord, cancellationToken).ConfigureAwait(false);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            OtherAccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.Commented1,
            TargetId = postRecord.AccountId,
            TargetPostId = postId,
            TargetCommentId = commentRecord.Id
        }, cancellationToken).ConfigureAwait(false);

        if (activeAccount.Id != postRecord.AccountId)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = postRecord.AccountId,
                OtherAccountId = activeAccount.Id,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                Type = AccountHistoryType.Commented2,
                TargetId = postRecord.AccountId,
                TargetPostId = postId,
                TargetCommentId = commentRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

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
            }, cancellationToken).ConfigureAwait(false);
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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        if (!activeAccount.CanInteract)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return 0;
        }

        var commentId = command.CommentId;
        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
        var allComments = allCommentPages[0].AllComments;
        if (!allComments.TryGetValue(commentId, out var commentViewModel))
        {
            return 0;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var commentRecord = database.PostComments.First(x => x.Id == postId);

        var newReaction = command.NewReaction;
        var reactionRecord = await database.PostCommentReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccount.Id && x.CommentId == commentId, cancellationToken).ConfigureAwait(false);
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

            await database.PostCommentReactions.AddAsync(reactionRecord, cancellationToken).ConfigureAwait(false);

            ModifyPostCommentWithReaction(commentRecord, newReaction, +1, true);
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

                ModifyPostCommentWithReaction(commentRecord, previousReaction, -1, newReaction == PostReaction.None);
            }

            if (newReaction != PostReaction.None)
            {
                reactionRecord.Reaction = newReaction;

                ModifyPostCommentWithReaction(commentRecord, newReaction, +1, previousReaction == PostReaction.None);
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
            }, cancellationToken).ConfigureAwait(false);

            if (activeAccount.Id != commentViewModel.AccountId)
            {
                await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
                {
                    AccountId = commentViewModel.AccountId,
                    OtherAccountId = activeAccount.Id,
                    //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                    Type = AccountHistoryType.ReactedToComment2,
                    TargetId = postRecord.AccountId,
                    TargetPostId = postRecord.Id,
                    TargetCommentId = commentId
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return null;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.PostVisibility = newVisibility;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);
        postRecord.DeletedTimeStamp = now;

        var imageBlobNames = postRecord.BlobNames.Split('|');
        foreach (var blobName in imageBlobNames)
        {
            var record = await database.UploadLogs.FirstOrDefaultAsync(x => x.AccountId == postRecord.AccountId && x.BlobName == blobName, cancellationToken).ConfigureAwait(false);
            if (record != null)
            {
                record.UploadStatus = AccountUploadLogStatus.DeletePending;
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            OtherAccountId = activeAccount.Id,
        }, cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return 0;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return 0;
        }

        var commentId = command.CommentId;
        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId, cancellationToken: cancellationToken).ConfigureAwait(false);
        commentRecord.DeletedTimeStamp = now;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = postRecord.AccountId,
            Type = AccountHistoryType.CommentDeleted,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = activeAccount.Id,
        }, cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(postRecord);

        var reportQueryResult = await database.PostReports.FirstOrDefaultAsync(r => r.PostId == postRecord.Id && r.AccountId == activeAccount.Id, cancellationToken).ConfigureAwait(false);
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
            postRecord.TotalReportCount++;
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
        }, cancellationToken).ConfigureAwait(false);

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return false;
        }

        var commentId = command.CommentId;
        var allCommentPages = await TryGetAllPostComments(postId).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var reportQueryResult = await database.PostCommentReports.FirstOrDefaultAsync(r => r.CommentId == commentId && r.AccountId == activeAccount.Id, cancellationToken).ConfigureAwait(false);
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

            var commentRecord = await database.PostComments.FirstAsync(x => x.Id == commentId, cancellationToken).ConfigureAwait(false);
            commentRecord.TotalReportCount++;
        }
        else
        {
            reportQueryResult.Reason = reason;
            reportQueryResult.ReasonText = reasonText;
            reportQueryResult.ModifiedTime = SystemClock.Instance.GetCurrentInstant();
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.PostReportedComment,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id,
            TargetCommentId = commentId,
            OtherAccountId = commentViewModel.AccountId,
        }, cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return false;
        }

        var postId = command.PostId;
        var postRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (postRecord == null)
        {
            return false;
        }

        var canSeePost = await CanAccountSeePost(activeAccount.Id, postRecord).ConfigureAwait(false);
        if (!canSeePost)
        {
            return false;
        }

        var allTagRecords = await GetAllPostTags(postId).ConfigureAwait(false);
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

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var reportQuery = from r in database.PostTagReports
                          where r.PostId == postRecord.Id && r.AccountId == activeAccount.Id
                          select r.TagId;

        var alreadyReported = await reportQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var alreadyReportedSet = alreadyReported.ToHashSet();

        foreach (var tagRecord in tagRecords)
        {
            if (alreadyReportedSet.Contains(tagRecord.Id))
            {
            }
            else
            {
                database.Attach(tagRecord);
                tagRecord.TotalReportCount++;

                await database.PostTagReports.AddAsync(new PostTagReportRecord
                {
                    AccountId = activeAccount.Id,
                    PostId = postRecord.Id,
                    TagId = tagRecord.Id,
                    CreatedTime = SystemClock.Instance.GetCurrentInstant()
                }, cancellationToken).ConfigureAwait(false);
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
        }, cancellationToken).ConfigureAwait(false);

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        var activeAccount = await _commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        if (!activeAccount.CanInteract)
        {
            return AddMemoryResultCode.SessionCanNotInteract;
        }

        var postId = command.PostId;
        var cachedPostRecord = await TryGetPostRecord(postId).ConfigureAwait(false);
        if (cachedPostRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var deletedTagKind = PostTagKind.DeletedByPoster;
        var accountViewModel = activeAccount;
        if (activeAccount.AccountType >= AccountType.Admin)
        {
            deletedTagKind = PostTagKind.DeletedByAdmin;
            accountViewModel = await _commonServices.AccountServices.TryGetAccountById(command.Session, cachedPostRecord.AccountId).ConfigureAwait(false);
        }
        else if (activeAccount.Id != cachedPostRecord.AccountId)
        {
            accountViewModel = null;
        }

        if (accountViewModel == null)
        {
            return AddMemoryResultCode.SessionNotFound;
        }

        await using var database = await CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var postRecord = await database.Posts.Include(x => x.PostTags).FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId, cancellationToken).ConfigureAwait(false);
        if (postRecord == null)
        {
            return AddMemoryResultCode.Failed;
        }

        var allCurrentTags = postRecord.PostTags.Where(x => x.IsPostTag).ToDictionary(x => x.TagString, x => x);
        var allActiveTags = allCurrentTags.Where(x => !x.Value.IsDeleted).Select(x => x.Key).ToHashSet();

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
                var newRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
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

                if (activeAccount.AccountType >= AccountType.Admin)
                {
                    
                }
                else if (newRecord.TagKind == PostTagKind.DeletedByAdmin)
                {
                    return AddMemoryResultCode.InvalidTags;
                }

                newRecord.PostId = postRecord.Id;
                newRecord.TagKind = PostTagKind.Post;

                allCurrentTags[newRecord.TagString] = newRecord;
            }

            foreach (var systemTag in removedSet)
            {
                if (allCurrentTags.TryGetValue(systemTag, out var tagRecord))
                {
                    if (tagRecord.TagKind == PostTagKind.DeletedByAdmin)
                    {
                    }
                    else
                    {
                        tagRecord.TagKind = deletedTagKind;
                    }
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
        var activeTags = postRecord.PostTags.Where(x => x.IsPostTag && !x.IsDeleted).Select(x => x.TagString).ToHashSet();
        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = postRecord.PostAvatar;
        }

        if (!string.IsNullOrWhiteSpace(avatar) && !activeTags.Contains(avatar))
        {
            avatar = null;
        }

        postRecord.PostAvatar = avatar;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

        return await query.ToDictionaryAsync(x => x.CommentId, x => x).ConfigureAwait(false);
    }

    [ComputeMethod]
    public virtual async Task<PostRecord> TryGetPostRecord(long postId)
    {
        await using var database = CreateDbContext();

        return await database.Posts.FirstOrDefaultAsync(p => p.DeletedTimeStamp == 0 && p.Id == postId).ConfigureAwait(false);
    }

    [ComputeMethod]
    protected virtual async Task<PostTagInfo[]> GetAllPostTagRecord(long postId, ServerSideLocale locale)
    {
        var allTagInfo = new List<PostTagInfo>();
        var allTagRecords = await GetAllPostTags(postId).ConfigureAwait(false);

        foreach (var tagRecord in allTagRecords)
        {
            string hashTagString = null;
            if (tagRecord.TagType == PostTagType.HashTag)
            {
                hashTagString = tagRecord.TagString.Split('-')[1];
            }

            var tagInfo = await _commonServices.TagServices.GetTagInfo(tagRecord.TagType, tagRecord.TagId, hashTagString, locale).ConfigureAwait(false);
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

        return await allTags.ToArrayAsync().ConfigureAwait(false);
    }

    private static void ModifyPostWithReaction(PostRecord record, PostReaction reaction, int change, bool modifyTotal)
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
                record.ReactionCount1 += change;
                break;
            }
            case PostReaction.Reaction2:
            {
                record.ReactionCount2 += change;
                break;
            }
            case PostReaction.Reaction3:
            {
                record.ReactionCount3 += change;
                break;
            }
            case PostReaction.Reaction4:
            {
                record.ReactionCount4 += change;
                break;
            }
            case PostReaction.Reaction5:
            {
                record.ReactionCount5 += change;
                break;
            }
            case PostReaction.Reaction6:
            {
                record.ReactionCount6 += change;
                break;
            }
            case PostReaction.Reaction7:
            {
                record.ReactionCount7 += change;
                break;
            }
            case PostReaction.Reaction8:
            {
                record.ReactionCount8 += change;
                break;
            }
            case PostReaction.Reaction9:
            {
                record.ReactionCount9 += change;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (modifyTotal)
        {
            record.TotalReactionCount += change;
        }
    }

    private static void ModifyPostCommentWithReaction(PostCommentRecord record, PostReaction reaction, int change, bool modifyTotal)
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
                record.ReactionCount1 += change;
                break;
            }
            case PostReaction.Reaction2:
            {
                record.ReactionCount2 += change;
                break;
            }
            case PostReaction.Reaction3:
            {
                record.ReactionCount3 += change;
                break;
            }
            case PostReaction.Reaction4:
            {
                record.ReactionCount4 += change;
                break;
            }
            case PostReaction.Reaction5:
            {
                record.ReactionCount5 += change;
                break;
            }
            case PostReaction.Reaction6:
            {
                record.ReactionCount6 += change;
                break;
            }
            case PostReaction.Reaction7:
            {
                record.ReactionCount7 += change;
                break;
            }
            case PostReaction.Reaction8:
            {
                record.ReactionCount8 += change;
                break;
            }
            case PostReaction.Reaction9:
            {
                record.ReactionCount9 += change;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (modifyTotal)
        {
            record.TotalReactionCount += change;
        }
    }
}