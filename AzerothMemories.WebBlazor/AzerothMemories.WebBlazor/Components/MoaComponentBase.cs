namespace AzerothMemories.WebBlazor.Components
{
    public abstract class MoaComponentBase<TViewModel> : ComputedStateComponent<TViewModel>, IMoaServices, IDisposable where TViewModel : ViewModelBase, new()
    {
        protected MoaComponentBase()
        {
            ViewModel = new TViewModel
            {
                Services = this
            };
        }

        public TViewModel ViewModel { get; }

        [Inject] public IAccountServices AccountServices { get; init; }

        [Inject] public ICharacterServices CharacterServices { get; init; }

        [Inject] public ActiveAccountServices ActiveAccountServices { get; init; }

        [Inject] public IStringLocalizer<BlizzardResources> StringLocalizer { get; init; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            ViewModel.OnViewModelChanged = EventCallback.Factory.Create(this, OnViewModelChanged);
        }

        protected override sealed async Task<TViewModel> ComputeState(CancellationToken cancellationToken)
        {
            await ActiveAccountServices.ComputeState(cancellationToken);

            await ViewModel.ComputeState(cancellationToken);

            return ViewModel;
        }

        private async Task OnViewModelChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            ViewModel.OnViewModelChanged = EventCallback.Empty;
        }
    }
}