namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryPublishComment : ISessionCommand<int>
{
    public Post_TryPublishComment(Session session, int postId, int parentCommentId, string commentText)
    {
        Session = session;
        PostId = postId;
        ParentCommentId = parentCommentId;
        CommentText = commentText;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int ParentCommentId { get; init; }

    [DataMember, MemoryPackInclude] public string CommentText { get; init; }
}