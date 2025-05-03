using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class CheckMemoryPageViewModel : ViewModelBase, IViewModel<CheckMemoryPageViewModel>
{
    private CheckMemoryInput[] _checkMemoryInfo = [];
    private CheckMemoryViewModel?[] _checkMemoryViewModels = [];
    private CheckMemoryResult? _checkMemoryResults;

    private CheckMemoryPageViewModel(IMoaServices services, Action onViewModelChanged) : base(services, onViewModelChanged)
    {
        AddMemoryPageViewModel = AddMemoryPageViewModel.CreateViewModel(services, onViewModelChanged);
    }

    public PostViewModel[] AllPostViewModels => _checkMemoryResults?.Posts ?? [];

    public CheckMemoryViewModel[] CheckMemoryViewModels => _checkMemoryViewModels.Where(x => x != null).ToArray()!;

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

        _checkMemoryInfo = checkMemoryInputs.OrderByDescending(x => x.ScreenShotUnixTime).ToArray();
        _checkMemoryViewModels = new CheckMemoryViewModel[_checkMemoryInfo.Length];

        OnViewModelChanged();
    }

    public override async Task ComputeState(CancellationToken cancellationToken)
    {
        await base.ComputeState(cancellationToken);

        var checkMemoryResult = await Services.ComputeServices.AccountServices.TryCheckMemory(Services.ClientServices.Session, ServerSideLocaleExt.GetServerSideLocale());
        for (var i = 0; i < _checkMemoryViewModels.Length; i++)
        {
            var checkMemoryInput = _checkMemoryInfo[i];
            if (checkMemoryInput.ScreenShotUnixTime > 0)
            {
                var (min, max) = ZExtensions.ClampTimeMinMaxAsLong(checkMemoryInput.ScreenShotUnixTime, 120);
                var currentPosts = checkMemoryResult.Posts.Where(x => x.PostTime > min && x.PostTime < max).ToArray();
                var currentAchievements = checkMemoryResult.Achievements.Where(x => x.TimeStamp > min && x.TimeStamp < max).Select(x => x.Achievement).ToHashSet(PostTagInfo.EqualityComparer1).ToArray();
                var matchingAchievements = 0;

                foreach (var achievement in currentAchievements)
                {
                    foreach (var postViewModel in currentPosts)
                    {
                        if (postViewModel.SystemTags.SafeEnumerable().ToHashSet(PostTagInfo.EqualityComparer1).Contains(achievement))
                        {
                            matchingAchievements++;
                        }
                    }
                }

                _checkMemoryViewModels[i] = new CheckMemoryViewModel
                {
                    Flags = checkMemoryInput.Flags,
                    ScreenShotUnixTime = checkMemoryInput.ScreenShotUnixTime,
                    BrowserFile = checkMemoryInput.BrowserFile.ThrowIfNull(),
                    CurrentPosts = currentPosts,
                    Achievements = currentAchievements,
                    MatchingAchievements = matchingAchievements,
                };
            }
        }

        _checkMemoryResults = checkMemoryResult;

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
        _checkMemoryInfo = [];
        _checkMemoryViewModels = [];

        return Task.CompletedTask;
    }

    public static CheckMemoryPageViewModel CreateViewModel(IMoaServices services, Action onViewModelChanged)
    {
        return new CheckMemoryPageViewModel(services, onViewModelChanged);
    }
}