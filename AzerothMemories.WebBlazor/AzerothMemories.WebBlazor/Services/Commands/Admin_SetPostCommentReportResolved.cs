namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Admin_SetPostCommentReportResolved : ISessionCommand<bool>
{
    public Admin_SetPostCommentReportResolved(Session session, bool delete, int postId, int commentId)
    {
        Session = session;
        Delete = delete;
        PostId = postId;
        CommentId = commentId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public bool Delete { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int CommentId { get; init; }
}