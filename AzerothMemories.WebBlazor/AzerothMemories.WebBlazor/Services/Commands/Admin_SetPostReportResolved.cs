namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Admin_SetPostReportResolved : ISessionCommand<bool>
{
    public Admin_SetPostReportResolved(Session session, bool delete, int postId)
    {
        Session = session;
        Delete = delete;
        PostId = postId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public bool Delete { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }
}