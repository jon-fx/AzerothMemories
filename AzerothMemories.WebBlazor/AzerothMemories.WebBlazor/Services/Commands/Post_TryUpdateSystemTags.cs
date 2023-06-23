namespace AzerothMemories.WebBlazor.Services.Commands;

[DataContract, MemoryPackable]
public sealed partial record Post_TryUpdateSystemTags : ISessionCommand<AddMemoryResultCode>
{
    public Post_TryUpdateSystemTags(Session session, int postId, string avatar, HashSet<string> newTags)
    {
        Session = session;
        PostId = postId;
        Avatar = avatar;
        NewTags = newTags;
    }

    [DataMember, MemoryPackInclude] public const string DefaultAvatar = "*USE-DEFAULT*";

    [DataMember, MemoryPackInclude] public Session Session { get; init; }

    [DataMember, MemoryPackInclude] public int PostId { get; init; }

    [DataMember, MemoryPackInclude] public string Avatar { get; init; }

    [DataMember, MemoryPackInclude] public HashSet<string> NewTags { get; init; }
}