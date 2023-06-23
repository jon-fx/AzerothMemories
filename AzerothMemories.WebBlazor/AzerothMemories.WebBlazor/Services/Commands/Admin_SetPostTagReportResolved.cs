namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Admin_SetPostTagReportResolved : ISessionCommand<bool>
{
    public Admin_SetPostTagReportResolved(Session session, bool delete, int postId, string tagString, int reportedTagId)
    {
        Session = session;
        Delete = delete;
        PostId = postId;
        TagString = tagString;
        ReportedTagId = reportedTagId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public bool Delete { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public string TagString { get; init; }

    [DataMember, MemoryPackInclude] public int ReportedTagId { get; init; }
}