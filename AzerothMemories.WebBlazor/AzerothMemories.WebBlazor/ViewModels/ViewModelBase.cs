namespace AzerothMemories.WebBlazor.ViewModels;

public abstract class ViewModelBase
{
    public IMoaServices Services { get; init; }

    public Action OnViewModelChanged { get; set; }

    public abstract Task ComputeState(CancellationToken cancellationToken);
}