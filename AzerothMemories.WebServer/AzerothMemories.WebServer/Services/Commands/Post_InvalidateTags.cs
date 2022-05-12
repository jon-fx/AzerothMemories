namespace AzerothMemories.WebServer.Services.Commands;

public sealed record Post_InvalidateTags(HashSet<string> TagStrings);