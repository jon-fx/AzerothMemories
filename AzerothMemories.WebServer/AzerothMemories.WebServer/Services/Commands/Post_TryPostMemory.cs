﻿namespace AzerothMemories.WebServer.Services.Commands;

public sealed record Post_TryPostMemory(Session Session, long TimeStamp, string AvatarTag, bool IsPrivate, string Comment, HashSet<string> SystemTags, List<byte[]> ImageData) : ISessionCommand<AddMemoryResult>
{
    public Post_TryPostMemory() : this(Session.Null, 0, null, false, null, new HashSet<string>(), new List<byte[]>())
    {
    }
}