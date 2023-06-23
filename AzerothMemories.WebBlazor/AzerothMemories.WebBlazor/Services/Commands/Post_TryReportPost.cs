namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryReportPost : ISessionCommand<bool>
{
    public Post_TryReportPost(Session session, int postId, PostReportedReason reason, string reasonText)
    {
        Session = session;
        PostId = postId;
        Reason = reason;
        ReasonText = reasonText;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public PostReportedReason Reason { get; init; }

    [DataMember, MemoryPackInclude] public string ReasonText { get; init; }
}