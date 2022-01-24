namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_TryChangeSocialLink(Session Session, int LinkId, string NewValue) : ICommand<string>
{
    public Account_TryChangeSocialLink() : this(Session.Null, 0, null)
    {
    }
}