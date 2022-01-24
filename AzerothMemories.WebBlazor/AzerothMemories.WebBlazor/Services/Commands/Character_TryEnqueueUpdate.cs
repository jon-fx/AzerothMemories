namespace AzerothMemories.WebBlazor.Services.Commands;

public record Character_TryEnqueueUpdate(Session Session) : ICommand<bool>
{
    public Character_TryEnqueueUpdate() : this(Session.Null)
    {
    }
}