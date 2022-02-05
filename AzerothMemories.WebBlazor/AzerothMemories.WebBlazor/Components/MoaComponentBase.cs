namespace AzerothMemories.WebBlazor.Components;

public abstract class MoaComponentBase<TViewModel> : ComputedStateComponent<TViewModel>, IMoaServices where TViewModel : ViewModelBase, new()
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

    [Inject] public ComputeServices ComputeServices { get; init; }

    [Inject] public ActiveAccountServices ActiveAccountServices { get; init; }

    [Inject] public TagHelpers TagHelpers { get; init; }

    [Inject] public TimeProvider TimeProvider { get; init; }

    [Inject] public UICommandRunner CommandRunner { get; init; }

    [Inject] public NavigationManager NavigationManager { get; init; }

    [Inject] public IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }

    [Inject] public DialogHelperService DialogService { get; init; }

    protected override sealed void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override sealed async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ViewModel.OnInitialized();
    }

    protected override sealed void OnParametersSet()
    {
        base.OnParametersSet();
    }

    protected override sealed Task OnParametersSetAsync()
    {
        return base.OnParametersSetAsync();
    }

    protected override sealed async Task<TViewModel> ComputeState(CancellationToken cancellationToken)
    {
        await ActiveAccountServices.ComputeState();

        await InternalComputeState();

        await ViewModel.ComputeState();

        return ViewModel;
    }

    protected virtual Task InternalComputeState()
    {
        return Task.CompletedTask;
    }
}