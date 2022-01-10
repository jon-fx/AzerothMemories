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

        public async Task ComputeState(string[] tagStrings, string sortModeString, string currentPageString, string minTimeString, string maxTimeString)
        {
            await PostSearchHelper.ComputeState(tagStrings, sortModeString, currentPageString, minTimeString, maxTimeString);
        }
    }
}