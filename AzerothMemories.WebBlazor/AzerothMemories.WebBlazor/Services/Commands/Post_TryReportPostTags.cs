namespace AzerothMemories.WebBlazor.Services.Commands;

public sealed record Post_TryReportPostTags(Session Session, int PostId, HashSet<string> TagStrings) : ISessionCommand<bool>;