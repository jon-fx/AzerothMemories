using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;

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

    public async Task<(AddMemoryResult Result, long PostId)> TryPostMemory(Session session, AddMemoryTransferData transferData)
    {
        const int maxLength = 2048;
        if (transferData.Comment.Length >= maxLength)
        {
            return (AddMemoryResult.CommentTooLong, 0);
        }

        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(transferData.TimeStamp);
        if (dateTimeOffset < DateTimeOffset.FromUnixTimeMilliseconds(946684800) || dateTimeOffset > DateTimeOffset.UtcNow)
        {
            return (AddMemoryResult.InvalidTime, 0);
        }

        var accountViewModel = await _commonServices.AccountServices.TryGetAccount(session);
        if (accountViewModel == null)
        {
            return (AddMemoryResult.SessionNotFound, 0);
        }

        if (!_commonServices.TagServices.GetCommentText(transferData.Comment, accountViewModel, out var commentText, out var accountsTaggedInComment, out var hashTagsTaggedInComment))
        {
            return (AddMemoryResult.ParseCommentFailed, 0);
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
        if (buildSystemTagsResult != AddMemoryResult.Success)
        {
            return (buildSystemTagsResult, 0);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, accountsTaggedInComment, hashTagsTaggedInComment, tagIds);
        if (addCommentTagResult != AddMemoryResult.Success)
        {
            return (addCommentTagResult, 0);
        }

        var uploadAndSortResult = await UploadAndSortImages(postRecord, transferData.UploadResults);
        if (uploadAndSortResult != AddMemoryResult.Success)
        {
            return (uploadAndSortResult, 0);
        }

        await using var database = _commonServices.DatabaseProvider.GetDatabase();
        postRecord.Id = await database.InsertWithInt64IdentityAsync(postRecord);

        var tagRecords = new List<PostTagRecord>();
        foreach (var tagId in tagIds)
        {
            tagRecords.Add(new PostTagRecord { PostId = postRecord.Id, TagId = tagId, CreatedTime = DateTimeOffset.UtcNow });
        }

        await database.PostTags.BulkCopyAsync(tagRecords);

        return (AddMemoryResult.Failed, 0);
    }

    private async Task<AddMemoryResult> BuildSystemTagsString(PostRecord postRecord, ActiveAccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<long> tagIds)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResult.InvalidTags;
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
                return AddMemoryResult.InvalidTags;
            }
        }

        postRecord.SystemTags = systemTagBuilder.ToString().TrimEnd('|');

        return AddMemoryResult.Success;
    }

    private async Task<AddMemoryResult> AddCommentTags(PostRecord postRecord, HashSet<long> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<long> tagIds)
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
                return AddMemoryResult.InvalidTags;
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
                return AddMemoryResult.InvalidTags;
            }
        }

        return AddMemoryResult.Success;
    }

    private async Task<AddMemoryResult> UploadAndSortImages(PostRecord postRecord, List<AddMemoryUploadResult> uploadResults)
    {
        var data = new List<(byte[] Buffer, string Hash, string Name, long TimeStamp, string ImageBlobName)>();
        foreach (var uploadResult in uploadResults)
        {
            if (uploadResult.FileContent.Length > 1024 * 1024 * 10)
            {
                return AddMemoryResult.Failed;
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
                return AddMemoryResult.Failed;
            }
        }

        var imageNameBuilder = new StringBuilder();
        foreach (var uploadResult in data)
        {
            imageNameBuilder.Append(uploadResult.ImageBlobName);
            imageNameBuilder.Append('|');
        }

        postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

        return AddMemoryResult.Success;
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