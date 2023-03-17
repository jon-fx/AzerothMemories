using Azure.Storage.Blobs;
using Humanizer;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Security.Cryptography;
using System.Text;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class PostServices_TryPostMemory
{
    public static async Task<AddMemoryResult> TryHandle(CommonServices commonServices, Post_TryPostMemory command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invPost = context.Operation().Items.Get<Post_InvalidatePost>();
            if (invPost != null && invPost.PostId > 0)
            {
                _ = commonServices.PostServices.DependsOnPost(invPost.PostId);
            }

            var invAccount = context.Operation().Items.Get<Post_InvalidateAccount>();
            if (invAccount != null && invAccount.AccountId > 0)
            {
                _ = commonServices.PostServices.DependsOnPostsBy(invAccount.AccountId);
                _ = commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = commonServices.PostServices.DependsOnPostsWithTagString(tagString);
                }
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = commonServices.PostServices.DependsOnNewPosts();
            }

            return default;
        }

        var commentText = command.Comment;
        if (string.IsNullOrWhiteSpace(commentText))
        {
            commentText = string.Empty;
        }

        if (commentText.Length >= ZExtensions.MaxPostCommentLength)
        {
            return new AddMemoryResult(AddMemoryResultCode.CommentTooLong);
        }

        var dateTime = Instant.FromUnixTimeMilliseconds(command.TimeStamp);
        if (dateTime < ZExtensions.MinPostTime || dateTime > SystemClock.Instance.GetCurrentInstant())
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTime);
        }

        var activeAccount = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (activeAccount == null)
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionNotFound);
        }

        if (!activeAccount.CanAddMemory())
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionCanNotInteract);
        }

        var parseResult = commonServices.MarkdownServices.GetCommentText(commentText, activeAccount.GetUserTagList(), true);
        if (parseResult.ResultCode != MarkdownParserResultCode.Success)
        {
            return new AddMemoryResult(AddMemoryResultCode.ParseCommentFailed);
        }

        //var linkStringBuilder = new StringBuilder();
        //foreach (var link in contextHelper.LinksInComment)
        //{
        //    linkStringBuilder.Append(link);
        //    linkStringBuilder.Append('|');
        //}

        var tagRecords = new HashSet<PostTagRecord>();
        var postRecord = new PostRecord
        {
            AccountId = activeAccount.Id,
            PostAvatar = command.AvatarTag,
            PostCommentRaw = parseResult.CommentText,
            PostCommentMark = parseResult.CommentTextMarkdown,
            PostCommentUserMap = parseResult.AccountsTaggedInCommentMap,
            PostTime = dateTime,
            PostCreatedTime = SystemClock.Instance.GetCurrentInstant(),
            PostEditedTime = SystemClock.Instance.GetCurrentInstant(),
            PostVisibility = command.IsPrivate ? (byte)1 : (byte)0,
        };

        var buildSystemTagsResult = await CreateSystemTags(commonServices, postRecord, activeAccount, command.SystemTags, tagRecords).ConfigureAwait(false);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(commonServices, postRecord, parseResult.AccountsTaggedInComment, parseResult.HashTagsTaggedInComment, tagRecords).ConfigureAwait(false);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        if (tagRecords.FirstOrDefault(x => x.TagString == PostTagInfo.GetTagString(PostTagType.Account, activeAccount.Id)) == null)
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTags);
        }

        //if (tagRecords.FirstOrDefault(x => x.TagString == PostTagInfo.GetTagString(PostTagType.Region, activeAccount.RegionId.ToValue())) == null)
        //{
        //    return new AddMemoryResult(AddMemoryResultCode.InvalidTags);
        //}

        if (!PostTagRecord.ValidateTagCounts(tagRecords))
        {
            return new AddMemoryResult(AddMemoryResultCode.InvalidTags);
        }

        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);

        var uploadAndSortResult = await UploadAndSortImages(commonServices, database, activeAccount, postRecord, command.ImageData, cancellationToken).ConfigureAwait(false);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        if (postRecord.Uploads == null || postRecord.Uploads.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(postRecord.PostCommentRaw))
            {
                return new AddMemoryResult(AddMemoryResultCode.NoImageMustContainText);
            }

            if (string.IsNullOrWhiteSpace(postRecord.PostCommentMark))
            {
                return new AddMemoryResult(AddMemoryResultCode.NoImageMustContainText);
            }
        }

        command.ImageData.Clear();

        postRecord.PostTags = tagRecords;

        await database.Posts.AddAsync(postRecord, cancellationToken).ConfigureAwait(false);
        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await commonServices.Commander.Call(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestored,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }, cancellationToken).ConfigureAwait(false);

        foreach (var userTag in parseResult.AccountsTaggedInComment)
        {
            await commonServices.Commander.Call(new Account_AddNewHistoryItem
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedPost,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        context.Operation().Items.Set(new Post_InvalidatePost(postRecord.Id));
        context.Operation().Items.Set(new Post_InvalidateAccount(activeAccount.Id));
        context.Operation().Items.Set(new Post_InvalidateTags(postRecord.PostTags.Select(x => x.TagString).ToHashSet()));

        if (postRecord.PostVisibility == 0)
        {
            context.Operation().Items.Set(new Post_InvalidateRecentPost(true));
        }

        return new AddMemoryResult(AddMemoryResultCode.Success, postRecord.AccountId, postRecord.Id);
    }

    private static async Task<AddMemoryResultCode> CreateSystemTags(CommonServices commonServices, PostRecord postRecord, AccountViewModel accountViewModel, HashSet<string> systemTags, HashSet<PostTagRecord> tagRecords)
    {
        if (!string.IsNullOrWhiteSpace(postRecord.PostAvatar) && !systemTags.Contains(postRecord.PostAvatar))
        {
            return AddMemoryResultCode.InvalidTags;
        }

        foreach (var systemTag in systemTags)
        {
            var tagRecord = await commonServices.TagServices.TryCreateTagRecord(systemTag, postRecord, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecord.TagKind = PostTagKind.Post;
            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private static async Task<AddMemoryResultCode> AddCommentTags(CommonServices commonServices, PostRecord postRecord, HashSet<int> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<PostTagRecord> tagRecords)
    {
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagRecord = await commonServices.TagServices.TryCreateTagRecord(postRecord, PostTagType.Account, accountId, PostTagKind.PostComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        foreach (var hashTag in hashTagsTaggedInComment)
        {
            var tagRecord = await commonServices.TagServices.GetHashTagRecord(hashTag, PostTagKind.PostComment).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private static async Task<AddMemoryResultCode> UploadAndSortImages(CommonServices commonServices, AppDbContext database, AccountViewModel accountViewModel, PostRecord postRecord, List<byte[]> imageDataList, CancellationToken cancellationToken)
    {
        if (imageDataList.Count > ZExtensions.MaxPostScreenShots)
        {
            return AddMemoryResultCode.UploadFailed;
        }

        var timeAgo = SystemClock.Instance.GetCurrentInstant() - commonServices.Config.UploadsInTheLastXDuration;
        var uploadsInTheLastX = database.UploadLogs.Count(x => x.AccountId == accountViewModel.Id && x.UploadTime > timeAgo);
        if (uploadsInTheLastX > commonServices.Config.UploadsInTheLastXCount)
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

                    await image.SaveAsGifAsync(memoryStream, encoder, cancellationToken).ConfigureAwait(false);
                    memoryStream.Position = 0;
                    extension = "gif";
                }
                else
                {
                    var defaultEncoder = new JpegEncoder();

                    await image.SaveAsJpegAsync(memoryStream, defaultEncoder, cancellationToken).ConfigureAwait(false);
                    memoryStream.Position = 0;

                    if (memoryStream.Length > 1.Megabytes().Bytes)
                    {
                        var secondEncoder = new JpegEncoder
                        {
                            Quality = accountViewModel.GetUploadQuality()
                        };

                        await image.SaveAsJpegAsync(memoryStream, secondEncoder, cancellationToken).ConfigureAwait(false);
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
            if (count > commonServices.Config.MaxUploadsWithTheSameHash)
            {
                return AddMemoryResultCode.UploadFailedHash;
            }
        }

        try
        {
            var imageNameBuilder = new StringBuilder();
            var postUploadLogs = new List<AccountUploadLog>();

            foreach (var (extension, blobData, blobHash) in dataToUpload)
            {
                var blobName = $"{accountViewModel.Id}-{Guid.NewGuid()}.{extension}";
                if (commonServices.Config.UploadToBlobStorage)
                {
                    bool blobExists;
                    BlobClient blobClient;
                    do
                    {
                        blobName = $"{accountViewModel.Id}-{Guid.NewGuid()}.{extension}";
                        blobClient = new BlobClient(commonServices.Config.BlobStorageConnectionString, ZExtensions.BlobUserUploads, blobName);
                        blobExists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
                    } while (blobExists);

                    var result = await blobClient.UploadAsync(blobData, cancellationToken).ConfigureAwait(false);
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

                postUploadLogs.Add(new AccountUploadLog
                {
                    Post = postRecord,
                    AccountId = accountViewModel.Id,
                    BlobName = blobName,
                    BlobHash = blobHash,
                    UploadTime = SystemClock.Instance.GetCurrentInstant()
                });
            }

            postRecord.Uploads = postUploadLogs;
            postRecord.BlobNames = imageNameBuilder.ToString().TrimEnd('|');

            return AddMemoryResultCode.Success;
        }
        catch (Exception)
        {
            return AddMemoryResultCode.UploadFailed;
        }
    }

    private static string CreateHashString(byte[] hashBytes)
    {
        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("X2"));
        }
        return sb.ToString();
    }
}