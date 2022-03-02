namespace AzerothMemories.WebServer.Services;

public record MediaResult(bool Success, string MediaType, byte[] MediaBytes)
{
}