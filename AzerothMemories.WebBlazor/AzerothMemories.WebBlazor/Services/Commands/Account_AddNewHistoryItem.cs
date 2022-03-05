namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_AddNewHistoryItem : ICommand<bool>
{
    public int AccountId { get; init; }

    public AccountHistoryType Type { get; init; }

    public int? OtherAccountId { get; init; }

    public int TargetId { get; init; }

    public int? TargetPostId { get; init; }

    public int? TargetCommentId { get; init; }
}