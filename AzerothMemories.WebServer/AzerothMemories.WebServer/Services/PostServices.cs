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
    protected virtual async Task<long> GetAccountIdOfPost(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from p in database.Posts
                    where p.Id == postId
                    select p.AccountId;

        return await query.FirstOrDefaultAsync();
    }

    //[ComputeMethod]
    //protected virtual async Task<bool> CanActiveUserSeePostsOf(Session session, long otherAccountId)
    //{
    //    var activeAccount = await _commonServices.AccountServices.TryGetAccount(session);
    //    long activeAccountId = 0;
    //    if (activeAccount != null)
    //    {
    //        activeAccountId = activeAccount.Id;
    //    }

    //    return await CanAccountIdSeePostsOf(activeAccountId, otherAccountId);
    //}

    [ComputeMethod]
    protected virtual async Task<bool> CanAccountIdSeePostsOf(long activeAccountId, long otherAccountId)
    {
        //if (posterAccount.IsPrivate)
        //{
        //    throw new NotImplementedException();
        //}

        return true;
    }

    public async Task<AddMemoryResult> TryPostMemory(Session session, AddMemoryTransferData transferData)
    {
        const int maxLength = 2048;
        if (transferData.Comment.Length >= maxLength)
        {
            return new AddMemoryResult(AddMemoryResultCode.CommentTooLong);
        }

        var dateTime = Instant.FromUnixTimeMilliseconds(transferData.TimeStamp);
        if (dateTime < Instant.FromUnixTimeMilliseconds(946684800) || dateTime > SystemClock.Instance.GetCurrentInstant())
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTime);
        }

        var accountViewModel = await _commonServices.AccountServices.TryGetAccount(session);
        if (accountViewModel == null)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionNotFound);
        }

        if (!_commonServices.TagServices.GetCommentText(transferData.Comment, accountViewModel, out var commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return new AddMemoryResult(AddMemoryResultCode.ParseCommentFailed);
        }

        var tagRecords = new HashSet<PostTagRecord>();
        var postRecord = new PostRecord
        {
            AccountId = accountViewModel.Id,
            PostAvatar = transferData.AvatarTag,
            PostComment = commentText,
            PostTime = dateTime,
            PostCreatedTime = SystemClock.Instance.GetCurrentInstant(),
            PostEditedTime = SystemClock.Instance.GetCurrentInstant(),
            PostVisibility = transferData.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await CreateSystemTags(postRecord, accountViewModel, transferData.SystemTags, tagRecords);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagRecords);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        if (tagRecords.Count > 64)
        {
            return new AddMemoryResult(AddMemoryResultCode.TooManyTags);
        }

        var uploadAndSortResult = await UploadAndSortImages(postRecord, transferData.UploadResults);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        postRecord.Id = await database.InsertWithInt64IdentityAsync(postRecord);

        foreach (var tagRecord in tagRecords)
        {
            tagRecord.PostId = postRecord.Id;
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        return new AddMemoryResult(AddMemoryResultCode.Success, postRecord.AccountId, postRecord.Id);
    }

    private async Task<AddMemoryResultCode> CreateSystemTags(PostRecord postRecord, ActiveAccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<PostTagRecord> tagRecords)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResultCode.InvalidTags;
        }

        foreach (var systemTag in systemTags)
        {
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, accountViewModel);
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
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(PostTagType.Account, accountId);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecord.TagKind = PostTagKind.Comment;
            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.GetHashTagRecord(hashTag);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecord.TagKind = PostTagKind.Comment;
            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> UploadAndSortImages(PostRecord postRecord, List<AddMemoryUploadResult> uploadResults)
    {
        var data = new List<(byte[] Buffer, string Hash, string Name, long TimeStamp, string ImageBlobName)>();
        foreach (var uploadResult in uploadResults)
        {
            if (uploadResult.FileContent.Length > 1024 * 1024 * 10)
            {
                return AddMemoryResultCode.Failed;
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
                return AddMemoryResultCode.Failed;
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
    public virtual async Task<PostViewModel> TryGetPostViewModel(Session session, long postAccountId, long postId, string locale = null, CancellationToken cancellationToken = default)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session, cancellationToken);
        var canSeePost = await CanAccountIdSeePostsOf(activeAccountId, postAccountId);
        if (!canSeePost)
        {
            return null;
        }

        var posterAccount = await _commonServices.AccountServices.TryGetAccountById(session, postAccountId, cancellationToken);
        if (posterAccount == null)
        {
            return null;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord.AccountId != postAccountId)
        {
            return null;
        }

        var postTagInfos = await GetAllPostTagRecord(postId, locale);
        var reactionRecords = await GetPostReactions(postId);

        reactionRecords.TryGetValue(activeAccountId, out var reactionViewModel);

        return RecordToViewModels.CreatePostViewModel(postRecord, posterAccount, reactionViewModel, postTagInfos);
    }

    [ComputeMethod]
    protected virtual async Task<Dictionary<long, PostReactionViewModel>> GetPostReactions(long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from r in database.PostReactions
                    where r.PostId == postId && r.Reaction != PostReaction.None
                    from a in database.Accounts.Where(x => x.Id == r.AccountId)
                    select new PostReactionViewModel
                    {
                        Id = r.Id,
                        AccountId = r.AccountId,
                        Reaction = r.Reaction,
                        AccountUsername = a.Username,
                        LastUpdateTime = r.LastUpdateTime.ToUnixTimeMilliseconds(),
                    };

        return await query.ToDictionaryAsync(x => x.AccountId, x => x);
    }

    public async Task<long> TryReactToPost(Session session, long postId, PostReaction newReaction)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        if (activeAccountId == 0)
        {
            return 0;
        }

        var posterAccountId = await GetAccountIdOfPost(postId);
        if (posterAccountId == 0)
        {
            return 0;
        }

        var canSeePost = await CanAccountIdSeePostsOf(activeAccountId, posterAccountId);
        if (!canSeePost)
        {
            return 0;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return 0;
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var postQuery = database.GetUpdateQuery(postRecord, out _);
        var reactionRecord = await database.PostReactions.FirstOrDefaultAsync(x => x.AccountId == activeAccountId && x.PostId == postId);
        if (reactionRecord == null)
        {
            if (newReaction == PostReaction.None)
            {
                return 0;
            }

            reactionRecord = new PostReactionRecord
            {
                AccountId = activeAccountId,
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

        using var computed = Computed.Invalidate();
        _ = GetPostRecord(postId);
        _ = GetPostReactions(postId);

        return reactionRecord.Id;
    }

    [ComputeMethod]
    public virtual async Task<PostReactionViewModel[]> TryGetReactions(Session session, long postId)
    {
        var activeAccountId = await _commonServices.AccountServices.TryGetActiveAccountId(session);
        var posterAccountId = await GetAccountIdOfPost(postId);
        if (posterAccountId == 0)
        {
            return null;
        }

        var canSeePost = await CanAccountIdSeePostsOf(activeAccountId, posterAccountId);
        if (!canSeePost)
        {
            return null;
        }

        //var postRecord = await GetPostRecord(postId);
        //if (postRecord == null)
        //{
        //    return null;
        //}

        var dict = await GetPostReactions(postId);
        return dict.Values.ToArray();
    }

    [ComputeMethod]
    public virtual async Task<PostCommentViewModel[]> TryGetComments(Session session, long postId)
    {
        return Array.Empty<PostCommentViewModel>();
    }

    public async Task<bool> TryRestoreMemory(Session session, long postId, long previousCharacterId, long newCharacterId)
    {
        var activeAccount = await _commonServices.AccountServices.TryGetAccount(session);
        if (activeAccount == null)
        {
            return false;
        }

        var posterAccountId = await GetAccountIdOfPost(postId);
        if (posterAccountId == 0)
        {
            return false;
        }

        var canSeePost = await CanAccountIdSeePostsOf(activeAccount.Id, posterAccountId);
        if (!canSeePost)
        {
            return false;
        }

        var postRecord = await GetPostRecord(postId);
        if (postRecord == null)
        {
            return false;
        }

        long? newAccountTag = newCharacterId > 0 ? activeAccount.Id : null;
        long? newCharacterTag = newCharacterId > 0 ? newCharacterId : null;

        long? accountTagToRemove = newCharacterId > 0 ? null : activeAccount.Id;
        long? characterTagToRemove = previousCharacterId > 0 ? previousCharacterId : null;
        if (activeAccount.Id == posterAccountId)
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

        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        await TryRestoreMemoryUpdate(database, postId, PostTagType.Account, accountTagToRemove, newAccountTag);
        await TryRestoreMemoryUpdate(database, postId, PostTagType.Character, characterTagToRemove, newCharacterTag);

        using var computed = Computed.Invalidate();
        _ = GetAllPostTags(postId);

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
            var tagInfo = await _commonServices.TagServices.GetTagInfo(tagRecord.TagType, tagRecord.TagId, locale);
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
                      where tag.PostId == postId /*&& tag.TagKind == PostTagKind.Post*/ && tag.TagKind != PostTagKind.Deleted
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
}