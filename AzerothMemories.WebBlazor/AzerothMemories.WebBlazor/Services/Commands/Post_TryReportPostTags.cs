namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryReportPostTags : ISessionCommand<bool>
{
    public Post_TryReportPostTags(Session session, int postId, HashSet<string> tagStrings)
    {
        Session = session;
        PostId = postId;
        TagStrings = tagStrings;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public HashSet<string> TagStrings { get; init; }
}