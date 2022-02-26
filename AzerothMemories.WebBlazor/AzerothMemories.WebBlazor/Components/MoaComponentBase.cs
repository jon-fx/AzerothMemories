namespace AzerothMemories.WebBlazor.Components;

public abstract class MoaComponentBase<TViewModel> : ComputedStateComponent<TViewModel>, IMoaServices, IDisposable where TViewModel : ViewModelBase, new()
{
    protected MoaComponentBase()
    {
        ViewModel = new TViewModel
        {
            Services = this,
            OnViewModelChanged = StateHasChanged
        };
    }

    protected TViewModel ViewModel { get; }

    [Inject] public ClientServices ClientServices { get; init; }

    [Inject] public ComputeServices ComputeServices { get; init; }

    protected override sealed void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override sealed async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        OnParametersChanged();

        await ViewModel.OnInitialized();
    }

    protected override sealed void OnParametersSet()
    {
        base.OnParametersSet();

        OnParametersChanged();
    }

    protected override sealed Task OnParametersSetAsync()
    {
        return base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await ClientServices.CookieHelper.Initialize(ClientServices.JsRuntime);
        }
    }

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