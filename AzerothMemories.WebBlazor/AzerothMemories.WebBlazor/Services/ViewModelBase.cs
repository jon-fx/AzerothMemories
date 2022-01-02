namespace AzerothMemories.WebBlazor.Services;

public abstract class ViewModelBase
{
    public IMoaServices Services { get; init; }

    public EventCallback OnViewModelChanged { get; set; }

    public abstract Task ComputeState(CancellationToken cancellationToken);
}