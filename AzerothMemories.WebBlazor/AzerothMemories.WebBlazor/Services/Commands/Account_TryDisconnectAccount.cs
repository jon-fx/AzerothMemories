namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Account_TryDisconnectAccount : ISessionCommand<bool>
{
    public Account_TryDisconnectAccount(Session session, string schema, string key)
    {
        Session = session;
        Schema = schema;
        Key = key;
    }

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public string Schema { get; init; }

    [DataMember, MemoryPackInclude] public string Key { get; init; }
}