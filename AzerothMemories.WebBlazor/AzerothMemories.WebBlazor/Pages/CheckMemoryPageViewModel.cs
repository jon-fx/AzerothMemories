using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class CheckMemoryPageViewModel : ViewModelBase, IViewModel<CheckMemoryPageViewModel>
{
    private CheckMemoryInput[] _checkMemoryInputs = [];
    private CheckMemoryViewModel[] _checkMemoryResults = [];

    private CheckMemoryPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        AddMemoryPageViewModel = AddMemoryPageViewModel.CreateViewModel(services, onViewModelChanged);
    }

    public CheckMemoryViewModel[] CheckMemoryResults => _checkMemoryResults.Where(x => x != null).ToArray();

    public bool IsButtonDisabled { get; set; }

    public AddMemoryPageViewModel AddMemoryPageViewModel { get; }

    public async Task Initialize(IReadOnlyList<IBrowserFile>? arg)
    {
        await Reset();

        IsButtonDisabled = true;

        var checkMemoryInputs = new List<CheckMemoryInput>();
        foreach (var file in arg.SafeEnumerable())
        {
            var flags = CheckMemoryFlags.None;
            var extension = Path.GetExtension(file.Name);
            if (ZExtensions.ValidUploadExtensions.Contains(extension))
            {
                flags |= CheckMemoryFlags.SupportedFileExtension;
            }

            if (Services.ClientServices.TimeProvider.TryGetTimeFromFileName(file.Name, out var screenShotUnixTime))
            {
                flags |= CheckMemoryFlags.ValidTimeFromFileName;
            }
            else
            {
                screenShotUnixTime = file.LastModified.ToUnixTimeMilliseconds();
            }

            checkMemoryInputs.Add(new CheckMemoryInput { Flags = flags, ScreenShotUnixTime = screenShotUnixTime, BrowserFile = file });
        }

        _checkMemoryInputs = checkMemoryInputs.OrderByDescending(x => x.ScreenShotUnixTime).ToArray();
        _checkMemoryResults = new CheckMemoryViewModel[_checkMemoryInputs.Length];

        OnViewModelChanged();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        for (var i = 0; i < _checkMemoryInputs.Length; i++)
        {
            var checkMemoryInput = _checkMemoryInputs[i];
            var result = await Services.ComputeServices.AccountServices.TryCheckMemory(Services.ClientServices.Session, checkMemoryInput.ScreenShotUnixTime, ServerSideLocaleExt.GetServerSideLocale());

            _checkMemoryResults[i] = new CheckMemoryViewModel
            {
                Flags = checkMemoryInput.Flags,
                ScreenShotUnixTime = checkMemoryInput.ScreenShotUnixTime,
                BrowserFile = checkMemoryInput.BrowserFile.ThrowIfNull(),
                CurrentPosts = result.CurrentPosts,
                Achievements = result.Achievements,
                MatchingAchievements = result.MatchingAchievements,
            };
        }

        if (Services.ClientServices.DialogService.IsLoadingDialogVisible)
        {
            Services.ClientServices.DialogService.HideLoadingDialog();
        }
    }

    public async Task AddBrowserFileToMemory(CheckMemoryViewModel rowContextItem)
    {
        if (AddMemoryPageViewModel.UploadedImages.Count == 0)
        {
            await AddMemoryPageViewModel.Initialize([rowContextItem.BrowserFile]);
        }
        else
        {
            await AddMemoryPageViewModel.UploadMoreImages([rowContextItem.BrowserFile]);
        }

        OnViewModelChanged();

        await Services.ClientServices.ScrollManager.ScrollIntoViewAsync("#moa-add-post-top", ScrollBehavior.Smooth);
    }

    public bool AddMemoryTimeIsNear(long unixTime)
    {
        var memoryTime = AddMemoryPageViewModel.SharedData?.PostTimeStamp;
        if (memoryTime == null)
        {
            return false;
        }

        var min = memoryTime.Value.Minus(Duration.FromSeconds(300));
        var max = memoryTime.Value.Plus(Duration.FromSeconds(300));

        var inputTime = Instant.FromUnixTimeMilliseconds(unixTime);

        return inputTime >= min && inputTime <= max;
    }

    private Task Reset()
    {
        _checkMemoryInputs = [];
        _checkMemoryResults = [];

        return Task.CompletedTask;
    }

    public static CheckMemoryPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new CheckMemoryPageViewModel(services, onViewModelChanged);
    }
}