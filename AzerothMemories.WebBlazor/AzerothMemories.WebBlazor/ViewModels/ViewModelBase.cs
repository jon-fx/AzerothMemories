namespace AzerothMemories.WebBlazor.ViewModels;

public abstract class ViewModelBase
{
    protected ViewModelBase(IMoaServices services, Action onViewModelChanged)
    {
        Services = services;
        OnViewModelChanged = onViewModelChanged;
    }

    public IMoaServices Services { get; }

    public Action OnViewModelChanged { get; }

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