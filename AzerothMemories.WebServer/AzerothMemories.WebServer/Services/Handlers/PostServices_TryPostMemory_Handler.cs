using System.Security.Cryptography;
using System.Text;
using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AzerothMemories.WebServer.Services.Handlers;

internal sealed class PostServices_TryPostMemory_Handler : IMoaCommandHandler<Post_TryPostMemory, AddMemoryResult>
{
    private readonly CommonServices _commonServices;
    private readonly Func<Task<AppDbContext>> _databaseContextGenerator;

    public PostServices_TryPostMemory_Handler(CommonServices commonServices, Func<Task<AppDbContext>> databaseContextGenerator)
    {
        _commonServices = commonServices;
        _databaseContextGenerator = databaseContextGenerator;
    }

    public async Task<AddMemoryResult> TryHandle(Post_TryPostMemory command)
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
                _ = _commonServices.PostServices.DependsOnPostsBy(invAccount.AccountId);
                _ = _commonServices.AccountServices.GetPostCount(invAccount.AccountId);
            }

            var invTags = context.Operation().Items.Get<Post_InvalidateTags>();
            if (invTags != null && invTags.TagStrings != null)
            {
                foreach (var tagString in invTags.TagStrings)
                {
                    _ = _commonServices.PostServices.DependsOnPostsWithTagString(tagString);
                }
            }

            var invRecentPosts = context.Operation().Items.Get<Post_InvalidateRecentPost>();
            if (invRecentPosts != null)
            {
                _ = _commonServices.PostServices.DependsOnNewPosts();
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

        if (!activeAccount.CanAddMemory())
        {
            return new AddMemoryResult(AddMemoryResultCode.SessionCanNotInteract);
        }

        var parseResult = _commonServices.MarkdownServices.GetCommentText(command.Comment, activeAccount.GetUserTagList());
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

        var buildSystemTagsResult = await CreateSystemTags(postRecord, activeAccount, command.SystemTags, tagRecords).ConfigureAwait(false);
        if (buildSystemTagsResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(buildSystemTagsResult);
        }

        var addCommentTagResult = await AddCommentTags(postRecord, parseResult.AccountsTaggedInComment, parseResult.HashTagsTaggedInComment, tagRecords).ConfigureAwait(false);
        if (addCommentTagResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(addCommentTagResult);
        }

        if (!PostTagRecord.ValidateTagCounts(tagRecords))
        {
            return new AddMemoryResult(AddMemoryResultCode.TooManyTags);
        }

        await using var database = await _databaseContextGenerator().ConfigureAwait(false);

        var uploadAndSortResult = await UploadAndSortImages(database, activeAccount, postRecord, command.ImageData).ConfigureAwait(false);
        if (uploadAndSortResult != AddMemoryResultCode.Success)
        {
            return new AddMemoryResult(uploadAndSortResult);
        }

        command.ImageData.Clear();

        postRecord.PostTags = tagRecords;

        await database.Posts.AddAsync(postRecord).ConfigureAwait(false);
        await database.SaveChangesAsync().ConfigureAwait(false);

        await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
        {
            AccountId = activeAccount.Id,
            //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
            Type = AccountHistoryType.MemoryRestored,
            TargetId = postRecord.AccountId,
            TargetPostId = postRecord.Id
        }).ConfigureAwait(false);

        foreach (var userTag in parseResult.AccountsTaggedInComment)
        {
            await _commonServices.AccountServices.AddNewHistoryItem(new Account_AddNewHistoryItem
            {
                AccountId = userTag,
                OtherAccountId = activeAccount.Id,
                Type = AccountHistoryType.TaggedPost,
                //CreatedTime = SystemClock.Instance.GetCurrentInstant(),
                TargetId = postRecord.AccountId,
                TargetPostId = postRecord.Id
            }).ConfigureAwait(false);
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
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(systemTag, postRecord, accountViewModel, PostTagKind.Post).ConfigureAwait(false);
            if (tagRecord == null)
            {
                return AddMemoryResultCode.InvalidTags;
            }

            tagRecord.TagKind = PostTagKind.Post;
            tagRecords.Add(tagRecord);
        }

        return AddMemoryResultCode.Success;
    }

    private async Task<AddMemoryResultCode> AddCommentTags(PostRecord postRecord, HashSet<int> accountsTaggedInComment, HashSet<string> hashTagsTaggedInComment, HashSet<PostTagRecord> tagRecords)
    {
        foreach (var accountId in accountsTaggedInComment)
        {
            var tagRecord = await _commonServices.TagServices.TryCreateTagRecord(postRecord, PostTagType.Account, accountId, PostTagKind.PostComment).ConfigureAwait(false);
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
            var postUploadLogs = new List<AccountUploadLog>();
            foreach (var (extension, blobData, blobHash) in dataToUpload)
            {
                var blobName = $"{accountViewModel.Id}-{Guid.NewGuid()}.{extension}";
                if (_commonServices.Config.UploadToBlobStorage)
                {
                    var blobClient = new Azure.Storage.Blobs.BlobClient(_commonServices.Config.BlobStorageConnectionString, ZExtensions.BlobUserUploads, blobName);
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

    private string CreateHashString(byte[] hashBytes)
    {
        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("X2"));
        }
        return sb.ToString();
    }
}