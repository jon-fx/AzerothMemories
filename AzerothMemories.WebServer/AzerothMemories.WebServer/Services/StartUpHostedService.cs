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
        await using var database = await _commonServices.DatabaseHub.CreateDbContext(true, cancellationToken).ConfigureAwait(false);

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

        var maxAchievement = await database.BlizzardData.Where(x => x.TagType == PostTagType.Achievement).MaxAsync(x => x.TagId, cancellationToken: cancellationToken).ConfigureAwait(false);
        var firstEverAchievements = await database.CharacterFirstAchievements.ToDictionaryAsync(x => x.AchievementId, x => x, cancellationToken: cancellationToken).ConfigureAwait(false);

        var rounded = (maxAchievement / 1000 + 1) * 1000;
        for (var i = 0; i < rounded; i++)
        {
            if (firstEverAchievements.TryGetValue(i, out _))
            {
            }
            else
            {
                database.CharacterFirstAchievements.Add(new CharacterFirstAchievementRecord
                {
                    AchievementId = i,
                    AchievementTimeStamp = Instant.MaxValue
                });
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}