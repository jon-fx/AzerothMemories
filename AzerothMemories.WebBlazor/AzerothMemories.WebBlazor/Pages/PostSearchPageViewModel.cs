namespace AzerothMemories.WebBlazor.Pages
{
    public sealed class PostSearchPageViewModel : ViewModelBase
    {
        public PostSearchPageViewModel()
        {
        }

        public PostSearchHelper PostSearchHelper { get; private set; }

        public override async Task OnInitialized()
        {
            await base.OnInitialized();

            PostSearchHelper = new PostSearchHelper(Services);
        }

        public async Task ComputeState(string[] tagStrings, string sortModeString, string currentPageString, string minTimeString, string maxTimeString)
        {
            await PostSearchHelper.ComputeState(tagStrings, sortModeString, currentPageString, minTimeString, maxTimeString);
        }
    }
}