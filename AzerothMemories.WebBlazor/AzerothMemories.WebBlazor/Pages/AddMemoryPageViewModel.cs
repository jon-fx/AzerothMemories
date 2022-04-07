using Microsoft.AspNetCore.Components.Forms;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class AddMemoryPageViewModel : ViewModelBase
{
    public AddMemoryPageViewModel()
    {
        UploadedImages = new List<AddMemoryImageData>();
    }

    public List<AddMemoryImageData> UploadedImages { get; }

    public bool MaxUploadReached => UploadedImages.Count >= ZExtensions.MaxPostScreenShots;

    public PublishCommentComponent PublishCommentComponent { get; set; }

    public AddMemoryComponentSharedData SharedData { get; private set; }

    public async Task Initialize(InputFileChangeEventArgs arg)
    {
        await Reset();

        await SharedData.InitializeAccount(() => Services.ClientServices.ActiveAccountServices.AccountViewModel);

        if (arg != null)
        {
            foreach (var file in arg.GetMultipleFiles())
            {
                await TryAddFile(file);
            }
        }

        if (UploadedImages.Count == 0)
        {
            await SharedData.SetPostTimeStamp(SystemClock.Instance.GetCurrentInstant());
        }
        else
        {
            var currentFileTimeStamp = UploadedImages[0].FileTimeStamp;

            await SharedData.SetPostTimeStamp(Instant.FromUnixTimeMilliseconds(currentFileTimeStamp));
        }

        OnViewModelChanged?.Invoke();
    }

    public async Task UploadMoreImages(InputFileChangeEventArgs arg)
    {
        if (MaxUploadReached)
        {
            return;
        }

        Services.ClientServices.DialogService.ShowLoadingDialog();

        foreach (var file in arg.GetMultipleFiles())
        {
            await TryAddFile(file);
        }

        Services.ClientServices.DialogService.HideLoadingDialog();
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

        var extension = Path.GetExtension(file.Name);
        if (!ZExtensions.ValidUploadExtensions.Contains(extension))
        {
            await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{extension} is not supported yet.");
            return;
        }

        var previous = UploadedImages.FirstOrDefault(x => x.FileName == file.Name);
        if (previous != null)
        {
            await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{file.Name} already added.");
            return;
        }

        byte[] buffer;
        try
        {
            await using var memoryStream = new MemoryStream();
            var stream = file.OpenReadStream(ZExtensions.MaxAddMemoryFileSizeInBytes);
            await stream.CopyToAsync(memoryStream);
            buffer = memoryStream.ToArray();
        }
        catch (Exception)
        {
            await Services.ClientServices.DialogService.ShowNotificationDialog(false, $"{file.Name} read failed.");
            return;
        }

        if (!Services.ClientServices.TimeProvider.TryGetTimeFromFileName(file.Name, out var screenShotUnixTime))
        {
            screenShotUnixTime = file.LastModified.ToUnixTimeMilliseconds();
        }

        var uploadResult = new AddMemoryImageData
        {
            FileName = file.Name,
            FileTimeStamp = screenShotUnixTime,
            FileContent = buffer,
            ContentType = file.ContentType,
        };

        UploadedImages.Add(uploadResult);
    }
}