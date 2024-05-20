namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Updates_TryResetUpdateStatus : ISessionCommand<bool>
{
    public Updates_TryResetUpdateStatus(Session session, int updateRecordId)
    {
        Session = session;
        UpdateRecordId = updateRecordId;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; } = null!;

    [DataMember, MemoryPackInclude] public int UpdateRecordId { get; init; }
}