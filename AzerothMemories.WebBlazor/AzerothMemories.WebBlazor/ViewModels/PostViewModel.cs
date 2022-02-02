namespace AzerothMemories.WebBlazor.ViewModels;

public sealed class PostViewModel
{
    [JsonIgnore] private PostViewModelBlobInfo[] _blobInfo;

    public PostViewModel()
    {
    }

    [JsonInclude] public long Id;

    [JsonInclude] public long AccountId;

    [JsonInclude] public string AccountAvatar;

    [JsonInclude] public string AccountUsername;

    [JsonInclude] public string PostAvatar;

    [JsonInclude] public string PostComment;

    [JsonInclude] public byte PostVisibility;

    [JsonInclude] public long PostTime;

    [JsonInclude] public long PostEditedTime;

    [JsonInclude] public long PostCreatedTime;

    [JsonInclude] public string[] ImageBlobNames;

    [JsonInclude] public PostTagInfo[] SystemTags;

    [JsonInclude] public long ReactionId;

    [JsonInclude] public PostReaction Reaction;

    [JsonInclude] public int TotalCommentCount;

    [JsonInclude] public int TotalReactionCount;

    [JsonInclude] public int[] ReactionCounters;

    [JsonInclude] public long DeletedTimeStamp;

    public PostViewModelBlobInfo[] GetImageBlobInfo()
    {
        if (_blobInfo == null)
        {
            var results = new List<PostViewModelBlobInfo>();
            var title = $"{AccountUsername}'s memory";
            var description = "false";
            if (string.IsNullOrWhiteSpace(PostComment))
            {
            }
            else if (PostComment.Length > 50)
            {
                description = $"{PostComment[..50]}...";
            }
            else
            {
                description = PostComment;
            }

            foreach (var imageBlobName in ImageBlobNames)
            {
                if (string.IsNullOrWhiteSpace(imageBlobName))
                {
                    continue;
                }

                var imageSource = $"{ZExtensions.BlobStoragePath}{imageBlobName}";

                results.Add(new PostViewModelBlobInfo { Title = title, Description = description, Source = imageSource });
            }

            _blobInfo = results.ToArray();
        }

        return _blobInfo;
    }
}