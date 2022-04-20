using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AccountServices_TryChangeAvatarUpload
{
    public static async Task<string> TryHandle(CommonServices commonServices, Account_TryChangeAvatarUpload command, CancellationToken cancellationToken)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invRecord = context.Operation().Items.Get<Account_InvalidateAccountRecord>();
            if (invRecord != null)
            {
                _ = commonServices.AccountServices.DependsOnAccountRecord(invRecord.Id);
                _ = commonServices.AccountServices.DependsOnAccountAvatar(invRecord.Id);
                _ = commonServices.MediaServices.TryGetUserAvatar($"{ZExtensions.AvatarBlobFilePrefix}{invRecord.Id}-0.jpg");
                _ = commonServices.MediaServices.TryGetUserAvatar($"{ZExtensions.AvatarBlobFilePrefix}{invRecord.Id}-1.jpg");
            }

            return default;
        }

        var accountViewModel = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (accountViewModel == null)
        {
            return null;
        }

        if (!accountViewModel.CanUploadAvatar())
        {
            return null;
        }

        var newAvatar = accountViewModel.Avatar;
        var avatarIndex = accountViewModel.AccountFlags.HasFlag(AccountFlags.SecondAvatarIndex) ? 1 : 0;
        try
        {
            var buffer = command.ImageData;
            var bufferCount = buffer.Length;
            if (bufferCount == 0 || bufferCount > ZExtensions.MaxAvatarFileSizeInBytes)
            {
                return newAvatar;
            }

            await using var memoryStream = new MemoryStream();
            using var image = Image.Load(buffer);
            image.Metadata.ExifProfile = null;

            var encoder = new JpegEncoder();

            await image.SaveAsJpegAsync(memoryStream, encoder, cancellationToken).ConfigureAwait(false);
            memoryStream.Position = 0;

            BinaryData dataToUpload = null;
            if (memoryStream.Length > 1.Megabytes().Bytes)
            {
                encoder.Quality = accountViewModel.GetUploadQuality();

                await image.SaveAsJpegAsync(memoryStream, encoder, cancellationToken).ConfigureAwait(false);
                memoryStream.Position = 0;

                dataToUpload = new BinaryData(memoryStream.ToArray());
            }

            var blobName = $"{ZExtensions.AvatarBlobFilePrefix}{accountViewModel.Id}-{avatarIndex}.jpg";
            if (commonServices.Config.UploadToBlobStorage && dataToUpload != null)
            {
                var blobClient = new Azure.Storage.Blobs.BlobClient(commonServices.Config.BlobStorageConnectionString, ZExtensions.BlobUserAvatars, blobName);
                var result = await blobClient.UploadAsync(dataToUpload, true, cancellationToken).ConfigureAwait(false);
                if (result.Value == null)
                {
                    return newAvatar;
                }
            }

            newAvatar = $"{ZExtensions.BlobUserAvatarsStoragePath}{blobName}";
        }
        catch (Exception)
        {
            return newAvatar;
        }

        if (accountViewModel.Avatar == newAvatar)
        {
            return accountViewModel.Avatar;
        }

        var accountRecord = await commonServices.AccountServices.TryGetActiveAccountRecord(command.Session).ConfigureAwait(false);
        await using var database = await commonServices.DatabaseHub.CreateCommandDbContext(cancellationToken).ConfigureAwait(false);
        database.Attach(accountRecord);
        accountRecord.Avatar = newAvatar;
        accountRecord.AccountFlags = avatarIndex == 0 ? accountRecord.AccountFlags | AccountFlags.SecondAvatarIndex : accountRecord.AccountFlags & ~AccountFlags.SecondAvatarIndex;

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        context.Operation().Items.Set(new Account_InvalidateAccountRecord(accountRecord.Id, accountRecord.Username, accountRecord.FusionId));

        return newAvatar;
    }
}