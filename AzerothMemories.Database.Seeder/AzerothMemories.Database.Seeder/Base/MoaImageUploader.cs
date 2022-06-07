using AzerothMemories.WebServer.Common;
using System.Security.Cryptography;
using System.Text;

namespace AzerothMemories.Database.Seeder.Base;

internal sealed class MoaImageUploader
{
    private readonly CommonConfig _commonConfig;
    private readonly MoaResourceWriter _resourceWriter;
    private readonly ILogger<MoaImageUploader> _logger;

    public MoaImageUploader(CommonConfig commonConfig, MoaResourceWriter resourceWriter, ILogger<MoaImageUploader> logger)
    {
        _logger = logger;
        _commonConfig = commonConfig;
        _resourceWriter = resourceWriter;
    }

    public async Task Upload()
    {
        var fileInfo = _resourceWriter.GetLocalMediaFileInfo("*");

        Exceptions.ThrowIf(fileInfo.Directory == null);
        Exceptions.ThrowIf(fileInfo.Directory.Parent == null);

        var indexFilePath = Path.Combine(fileInfo.Directory.Parent.FullName, "uploadedMedia.txt");
        if (!File.Exists(indexFilePath))
        {
            await using var _ = File.Create(indexFilePath);
        }

        const string splitKey = "|";
        var uploadedFileLines = await File.ReadAllLinesAsync(indexFilePath);
        var alreadyUploadedHashData = (from line in uploadedFileLines
                                       let splt = line.Split(splitKey)
                                       let key = splt[0]
                                       let hash = splt[1]
                                       select new { Key = key, Hash = hash }).ToDictionary(x => x.Key, x => x.Hash);

        var csvRequiresRewrite = false;
        var files = fileInfo.Directory.EnumerateFiles("*.jpg").ToArray();
        foreach (var file in files)
        {
            var buffer = await File.ReadAllBytesAsync(file.FullName);
            var currentFileHash = GetFileHash(buffer);

            var fileName = file.Name;
            var alreadyUploaded = alreadyUploadedHashData.TryGetValue(fileName, out var uploadedFileHash);
            var hashesAreTheSame = uploadedFileHash == currentFileHash;

            if (alreadyUploaded && hashesAreTheSame)
            {
                _logger.LogDebug($"Skipping: {fileName} Hash: {currentFileHash}");
            }
            else
            {
                _logger.LogInformation($"Uploading {fileName} Hash: {currentFileHash}");

                try
                {
                    var blobClient = new Azure.Storage.Blobs.BlobClient(_commonConfig.BlobStorageConnectionString, ZExtensions.BlobStaticMedia, fileName);
                    var result = await blobClient.UploadAsync(new BinaryData(buffer), true).ConfigureAwait(false);
                    if (result.Value == null)
                    {
                    }
                    else
                    {
                        alreadyUploadedHashData[fileName] = currentFileHash;

                        if (alreadyUploaded)
                        {
                            csvRequiresRewrite = true;
                        }
                        else
                        {
                            await File.AppendAllLinesAsync(indexFilePath, new[] { $"{fileName}{splitKey}{currentFileHash}" });
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error");
                }
            }
        }

        if (csvRequiresRewrite)
        {
            var newFileLines = alreadyUploadedHashData.Select(kvp => $"{kvp.Key}{splitKey}{kvp.Value}").ToArray();
            await File.WriteAllLinesAsync(indexFilePath, newFileLines);
        }
    }

    private string GetFileHash(byte[] buffer)
    {
        var hashBytes = MD5.HashData(buffer);
        var hashBuildler = new StringBuilder();
        foreach (var b in hashBytes)
        {
            hashBuildler.Append(b.ToString("X2"));
        }

        return hashBuildler.ToString();
    }
}