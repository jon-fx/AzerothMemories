namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryDeleteComment : ISessionCommand<long>
{
    public Post_TryDeleteComment(Session session, int postId, int commentId)
    {
        Session = session;
        PostId = postId;
        CommentId = commentId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int CommentId { get; init; }
}