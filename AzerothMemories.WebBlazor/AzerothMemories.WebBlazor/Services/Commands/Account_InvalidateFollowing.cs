using System.Reactive;

namespace AzerothMemories.WebBlazor.Services.Commands;

public record Account_InvalidateFollowing(long AccountId, int Page) : ICommand<Unit>
{
    public Account_InvalidateFollowing() : this(0, 0)
    {
    }
}