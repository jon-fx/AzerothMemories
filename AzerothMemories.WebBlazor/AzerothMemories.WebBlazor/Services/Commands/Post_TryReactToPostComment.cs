namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryReactToPostComment : ISessionCommand<int>
{
    public Post_TryReactToPostComment(Session session, int postId, int commentId, PostReaction newReaction)
    {
        Session = session;
        PostId = postId;
        CommentId = commentId;
        NewReaction = newReaction;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public int CommentId { get; init; }

    [DataMember, MemoryPackInclude] public PostReaction NewReaction { get; init; }
}