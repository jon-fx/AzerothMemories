﻿namespace AzerothMemories.WebBlazor.Components;

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

    [Inject] public IAccountServices AccountServices { get; init; }

    [Inject] public IAccountFollowingServices AccountFollowingServices { get; init; }

    [Inject] public ICharacterServices CharacterServices { get; init; }

    [Inject] public ITagServices TagServices { get; init; }

    [Inject] public IPostServices PostServices { get; init; }

    [Inject] public ISearchPostsServices SearchPostsServices { get; set; }

    [Inject] public ActiveAccountServices ActiveAccountServices { get; init; }

    [Inject] public TagHelpers TagHelpers { get; init; }

    [Inject] public TimeProvider TimeProvider { get; init; }

    [Inject] public NavigationManager NavigationManager { get; init; }

    [Inject] public IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }

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
        await ActiveAccountServices.ComputeState(cancellationToken);

        await InternalComputeState();

        await ViewModel.ComputeState();

        return ViewModel;
    }

    protected virtual Task InternalComputeState()
    {
        return Task.CompletedTask;
    }
}