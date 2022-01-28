namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_AddNewHistoryItem : ICommand<bool>
{
    public long AccountId { get; init; }

    public AccountHistoryType Type { get; init; }

    public long? OtherAccountId { get; init; }

    public long TargetId { get; init; }

    public long? TargetPostId { get; init; }

    public long? TargetCommentId { get; init; }
}