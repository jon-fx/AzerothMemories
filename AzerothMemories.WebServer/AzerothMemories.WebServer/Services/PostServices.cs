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

        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(transferData.TimeStamp);
        if (dateTimeOffset < DateTimeOffset.FromUnixTimeMilliseconds(946684800) || dateTimeOffset > DateTimeOffset.UtcNow)
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

        var tagIds = new HashSet<long>();
        var postRecord = new PostRecord
        {
            AccountId = accountViewModel.Id,
            PostAvatar = transferData.AvatarTag,
            PostComment = commentText,
            PostTime = dateTimeOffset,
            PostEditedTime = dateTimeOffset,
            PostCreatedTime = dateTimeOffset,
            PostVisibility = transferData.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await BuildSystemTagsString(postRecord, accountViewModel, transferData.SystemTags, tagIds);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagIds);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        var uploadAndSortResult = await UploadAndSortImages(postRecord, transferData.UploadResults);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        postRecord.Id = await database.InsertWithInt64IdentityAsync(postRecord);

        var tagRecords = new List<PostTagRecord>();
        foreach (var tagId in tagIds)
        {
            tagRecords.Add(new PostTagRecord { PostId = postRecord.Id, TagId = tagId, CreatedTime = DateTimeOffset.UtcNow });
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        return new AddMemoryResult(AddMemoryResultCode.Success, postRecord.AccountId, postRecord.Id);
    }

    private async Task<AddMemoryResultCode> BuildSystemTagsString(PostRecord postRecord, ActiveAccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<long> tagIds)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResultCode.InvalidTags;
        }

        var systemTagBuilder = new StringBuilder();
        foreach (var systemTag in systemTags)
        {
            var tagId = await _commonServices.TagServices.IsValidTags(systemTag, accountViewModel);
            if (tagId > 0 && tagIds.Add(tagId))
            {
                systemTagBuilder.Append(systemTag);
                systemTagBuilder.Append("~0|");
            }
            else
            {
                return AddMemoryResultCode.InvalidTags;
            }
        }

        postRecord.SystemTags = systemTagBuilder.ToString().TrimEnd('|');

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> AddCommentTags(PostRecord postRecord, HashSet<long> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<long> tagIds)
    {
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagId = await _commonServices.TagServices.GetTagRecordId(PostTagType.Account, accountId);
            if (tagId > 0)
            {
                tagIds.Add(tagId);
            }
            else
            {
                return AddMemoryResultCode.InvalidTags;
            }
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagId = await _commonServices.TagServices.GetHashTagRecordId(hashTag);
            if (tagId > 0)
            {
                tagIds.Add(tagId);
            }
            else
            {
                return AddMemoryResultCode.InvalidTags;
            }
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
}