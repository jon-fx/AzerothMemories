namespace AzerothMemories.WebBlazor.Components
{
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

        public TViewModel ViewModel { get; }

        [Inject] public IAccountServices AccountServices { get; init; }

        [Inject] public ICharacterServices CharacterServices { get; init; }

        [Inject] public ITagServices TagServices { get; init; }

        [Inject] public ActiveAccountServices ActiveAccountServices { get; init; }

        [Inject] public TagHelpers TagHelpers { get; init; }

        [Inject] public TimeProvider TimeProvider { get; init; }

        [Inject] public IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }

        protected override sealed async Task<TViewModel> ComputeState(CancellationToken cancellationToken)
        {
            await ActiveAccountServices.ComputeState(cancellationToken);

            await ViewModel.ComputeState(cancellationToken);

            return ViewModel;
        }
    }
}