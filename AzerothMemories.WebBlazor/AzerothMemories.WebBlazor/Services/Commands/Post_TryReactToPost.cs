namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryReactToPost : ISessionCommand<int>
{
    public Post_TryReactToPost(Session session, int postId, PostReaction newReaction)
    {
        Session = session;
        PostId = postId;
        NewReaction = newReaction;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public PostReaction NewReaction { get; init; }
}