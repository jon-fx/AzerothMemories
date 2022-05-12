namespace AzerothMemories.WebServer.Services.Commands;

public record Account_TryChangeAvatarUpload(Session Session, byte[] ImageData) : ISessionCommand<string>;