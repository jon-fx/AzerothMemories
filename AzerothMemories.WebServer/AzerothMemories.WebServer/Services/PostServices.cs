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
        var activeAccount = await _commonServices.AccountServices.GetCurrentSessionAccountRecord(session, cancellationToken);
        long activeAccountId = 0;
        if (activeAccount != null)
        {
            activeAccountId = activeAccount.Id;
        }

        var posterAccount = await _commonServices.AccountServices.TryGetAccountById(session, postAccountId, cancellationToken);
        if (posterAccount == null)
        {
            return null;
        }

        if (posterAccount.IsPrivate)
        {
            throw new NotImplementedException();
        }

        var postRecord = await GetPostRecord(postAccountId, postId);
        var postTagInfos = await GetAllPostTagRecord(postId, locale);

        return RecordToViewModels.CreatePostViewModel(postRecord, posterAccount, null, postTagInfos);
    }

    [ComputeMethod]
    protected virtual async Task<PostRecord> GetPostRecord(long postAccountId, long postId)
    {
        await using var database = _commonServices.DatabaseProvider.GetDatabase();

        var query = from p in database.Posts
                    where p.DeletedTimeStamp == 0 && p.Id == postId && p.AccountId == postAccountId
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
                      where tag.PostId == postId /*&& tag.TagKind == PostTagKind.Post*/
                      select tag;

        return await allTags.ToArrayAsync();
    }
}