namespace AzerothMemories.WebServer.Services;

internal sealed class StartUpHostedService : IHostedService
{
    private readonly CommonServices _commonServices;

    public StartUpHostedService(CommonServices commonServices)
    {
        _commonServices = commonServices;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var database = await _commonServices.DatabaseHub.CreateDbContext(cancellationToken).ConfigureAwait(false);

        var postTags = await database.PostTags.ToArrayAsync(cancellationToken).ConfigureAwait(false);

        foreach (var postTag in postTags)
        {
            switch (postTag.TagType)
            {
                case PostTagType.None:
                case PostTagType.HashTag:
                case PostTagType.Account:
                case PostTagType.Character:
                case PostTagType.Guild:
                {
                    break;
                }
                default:
                {
                    var record = await _commonServices.TagServices.GetBlizzardDataRecord(postTag.TagString).ConfigureAwait(false);
                    if (record == null)
                    {
                    }
                    break;
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}