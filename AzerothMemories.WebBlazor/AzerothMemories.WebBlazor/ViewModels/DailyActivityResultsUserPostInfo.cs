namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsUserPostInfo
{
    public int AccountId { get; init; }
    public int PostId { get; init; }
    public long PostTime { get; init; }
    public long PostCreatedTime { get; init; }
    public PostViewModelBlobInfo[] BlobInfo { get; init; }

    public PostViewModelBlobInfo[] GetBlobPreviewInfo(string description)
    {
        var results = new PostViewModelBlobInfo[BlobInfo.Length];
        for (var i = 0; i < results.Length; i++)
        {
            var blobInfo = BlobInfo[i];
            var newBlobInfo = blobInfo with
            {
                Title = blobInfo.Title + $": {i + 1}/{BlobInfo.Length}",
                Description = description,
                Source = blobInfo.Source
            };

            results[i] = newBlobInfo;
        }

        return results;
    }
}