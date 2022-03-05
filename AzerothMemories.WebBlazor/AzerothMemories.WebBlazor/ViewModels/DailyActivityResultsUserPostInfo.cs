namespace AzerothMemories.WebBlazor.ViewModels;

public sealed record DailyActivityResultsUserPostInfo
{
    public int AccountId { get; init; }
    public int PostId { get; init; }
    public long PostTime { get; init; }
    public PostViewModelBlobInfo[] BlobInfo { get; init; }
}