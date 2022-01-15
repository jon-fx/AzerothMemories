using AzerothMemories.WebBlazor.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class AddMemoryPageViewModel : ViewModelBase
{
    public AddMemoryPageViewModel()
    {
        UploadedImages = new List<AddMemoryUploadResult>();
    }

    public List<AddMemoryUploadResult> UploadedImages { get; }

    public bool MaxUploadReached => UploadedImages.Count > 3;

    public PublishCommentComponent PublishCommentComponent { get; set; }

    public AddMemoryComponentSharedData SharedData { get; private set; }

    public async Task Initialize(InputFileChangeEventArgs arg)
    {
        await Reset();

        await SharedData.InitializeAccount(() => Services.ActiveAccountServices.AccountViewModel);

        foreach (var file in arg.GetMultipleFiles())
        {
            await TryAddFile(file);
        }

        if (UploadedImages.Count > 0)
        {
            var currentFileTimeStamp = UploadedImages[0].FileTimeStamp;

            await SharedData.SetPostTimeStamp(Instant.FromUnixTimeMilliseconds(currentFileTimeStamp));

            OnViewModelChanged?.Invoke();
        }
    }

    public async Task UploadMoreImages(InputFileChangeEventArgs arg)
    {
        if (MaxUploadReached)
        {
            return;
        }

        //await TimeProvider.EnsureInitialized(CancellationToken);

        Services.DialogService.ShowLoadingDialog();

        foreach (var file in arg.GetMultipleFiles())
        {
            await TryAddFile(file);
        }

        Services.DialogService.HideLoadingDialog();
    }

    public Task<AddMemoryResult> Submit()
    {
        return SharedData.Submit(PublishCommentComponent, UploadedImages);
    }

    public Task Reset()
    {
        UploadedImages.Clear();
        PublishCommentComponent = null;
        SharedData = new AddMemoryComponentSharedData(this);

        return Task.CompletedTask;
    }

    private async Task TryAddFile(IBrowserFile file)
    {
        if (MaxUploadReached)
        {
            return;
        }

        var previous = UploadedImages.FirstOrDefault(x => x.FileName == file.Name);
        if (previous != null)
        {
            return;
        }

        await using var memoryStream = new MemoryStream();
        var stream = file.OpenReadStream(5120000 * 2);
        await stream.CopyToAsync(memoryStream);

        var buffer = memoryStream.ToArray();
        var contentBase64 = Convert.ToBase64String(buffer);
        previous = UploadedImages.FirstOrDefault(x => x.ContentBase64 == contentBase64);
        if (previous != null)
        {
            return;
        }

        if (!Services.TimeProvider.TryGetTimeFromFileName(file.Name, out var screenShotUnixTime))
        {
            screenShotUnixTime = file.LastModified.ToUnixTimeMilliseconds();
        }

        var uploadResult = new AddMemoryUploadResult
        {
            FileName = file.Name,
            FileTimeStamp = screenShotUnixTime,
            FileContent = buffer,
            ContentType = file.ContentType,
            ContentBase64 = contentBase64
        };

        UploadedImages.Add(uploadResult);
    }
}