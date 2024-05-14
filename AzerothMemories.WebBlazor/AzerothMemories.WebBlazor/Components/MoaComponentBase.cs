namespace AzerothMemories.WebBlazor.Components;

public abstract class MoaComponentBase<TViewModel> : ComputedStateComponent<TViewModel>, IMoaServices, IDisposable where TViewModel : ViewModelBase, IViewModel<TViewModel>
{
    protected MoaComponentBase()
    {
        ViewModel = TViewModel.CreateViewModel(this, StateHasChanged);
    }

    protected TViewModel ViewModel { get; }

    [Inject] public ClientServices ClientServices { get; init; } = null!;

    [Inject] public ComputeServices ComputeServices { get; init; } = null!;

    protected override sealed void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override sealed async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        OnParametersChanged();

        await ViewModel.OnInitialized();

        await State.Update();
    }

    protected override sealed void OnParametersSet()
    {
        base.OnParametersSet();
    }

    protected override sealed async Task OnParametersSetAsync()
    {
        OnParametersChanged();

        await base.OnParametersSetAsync();
    }

    //protected override Task OnAfterRenderAsync(bool firstRender)
    //{
    //    return base.OnAfterRenderAsync(firstRender);
    //}

    protected virtual void OnParametersChanged()
    {
    }

    protected override sealed async Task<TViewModel> ComputeState(CancellationToken cancellationToken)
    {
        await ClientServices.ActiveAccountServices.ComputeState();

        await ViewModel.ComputeState(cancellationToken);

        await OnComputeState(cancellationToken);

        return ViewModel;
    }

    protected virtual Task OnComputeState(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        ViewModel.Dispose();
    }
}