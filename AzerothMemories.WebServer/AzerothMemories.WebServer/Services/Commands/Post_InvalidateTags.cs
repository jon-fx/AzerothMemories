namespace AzerothMemories.WebServer.Services.Commands;

public record Post_InvalidateTags(HashSet<string> TagStrings)
{
    public Post_InvalidateTags() : this(new HashSet<string>())
    {
    }
}