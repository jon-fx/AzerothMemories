namespace AzerothMemories.WebBlazor.ViewModels;

public abstract class ViewModelBase
{
    public IMoaServices Services { get; init; }

    public Action OnViewModelChanged { get; set; }

    public virtual Task OnInitialized()
    {
        return Task.CompletedTask;
    }

    public virtual Task ComputeState(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }
}