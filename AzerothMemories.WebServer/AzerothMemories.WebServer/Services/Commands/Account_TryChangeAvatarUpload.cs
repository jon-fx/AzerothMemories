namespace AzerothMemories.WebServer.Services.Commands;

public record Account_TryChangeAvatarUpload(Session Session, byte[] ImageData) : ISessionCommand<string>
{
    public Account_TryChangeAvatarUpload() : this(Session.Null, null)
    {
    }
}