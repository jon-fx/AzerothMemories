namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsUserPostInfo
{
    public long AccountId { get; init; }
    public long PostId { get; init; }
    public long PostTime { get; init; }
    public PostViewModelBlobInfo[] BlobInfo { get; init; }
}