﻿namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeSocialLink(Session Session, long AccountId, int LinkId, string NewValue) : ISessionCommand<string>
{
    public Account_TryChangeSocialLink() : this(Session.Null, 0, 0, null)
    {
    }
}