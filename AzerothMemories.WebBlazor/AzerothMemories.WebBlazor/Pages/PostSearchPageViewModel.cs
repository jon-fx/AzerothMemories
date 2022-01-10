namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class PostSearchPageViewModel : ViewModelBase
    {
        public PostSearchPageViewModel()
        {
        }

        public PostSearchHelper PostSearchHelper { get; private set; }

        public void OnInitialized()
        {
            PostSearchHelper = new PostSearchHelper(Services);
        }

        public async Task OnParametersSet(string[] tagStrings, string sortModeString, string currentPageString, string minTimeString, string maxTimeString)
        {
            await PostSearchHelper.OnParametersSetAsync(tagStrings, sortModeString, currentPageString, minTimeString, maxTimeString);
        }

        public override async Task ComputeState(CancellationToken cancellationToken)
        {
            await PostSearchHelper.OnComputeState();
        }
    }
}