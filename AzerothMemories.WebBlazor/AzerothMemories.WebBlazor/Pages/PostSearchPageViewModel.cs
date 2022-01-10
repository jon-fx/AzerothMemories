namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class PostSearchPageViewModel : ViewModelBase
    {
        public PostSearchPageViewModel()
        {
        }

        public PostSearchHelper PostSearchHelper { get; private set; }

        public override Task OnInitialized()
        {
            PostSearchHelper = new PostSearchHelper(Services);

            return Task.CompletedTask;
        }

        public async Task OnParametersChanged(string[] tagStrings, string sortModeString, string currentPageString, string minTimeString, string maxTimeString)
        {
            await PostSearchHelper.OnParametersChanged(tagStrings, sortModeString, currentPageString, minTimeString, maxTimeString);
        }

        public override async Task ComputeState()
        {
            await PostSearchHelper.OnComputeState();
        }
    }
}