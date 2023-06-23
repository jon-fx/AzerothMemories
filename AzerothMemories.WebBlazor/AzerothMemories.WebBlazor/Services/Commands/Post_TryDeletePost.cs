namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryDeletePost : ISessionCommand<long>
{
    public Post_TryDeletePost(Session session, int postId)
    {
        Session = session;
        PostId = postId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }
}