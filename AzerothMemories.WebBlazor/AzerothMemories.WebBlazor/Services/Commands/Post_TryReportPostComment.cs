namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryReportPostComment : ISessionCommand<bool>
{
    public Post_TryReportPostComment(Session session, int postId, int commentId, PostReportedReason reason, string reasonText)
    {
        Session = session;
        PostId = postId;
        CommentId = commentId;
        Reason = reason;
        ReasonText = reasonText;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int CommentId { get; init; }

    [DataMember, MemoryPackInclude] public PostReportedReason Reason { get; init; }

    [DataMember, MemoryPackInclude] public string ReasonText { get; init; }
}