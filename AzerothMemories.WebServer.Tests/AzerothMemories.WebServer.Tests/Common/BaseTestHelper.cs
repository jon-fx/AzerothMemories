using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Common;

public class BaseTestHelper : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly CommonServices _commonServices;

    private int _userId = 1;
    private int _characterId = 1;

    public BaseTestHelper()
    {
        _serviceProvider = GetServiceProvider();
        _commonServices = _serviceProvider.GetRequiredService<CommonServices>();
    }

    protected IServiceProvider ServiceProvider => _serviceProvider;

    protected CommonServices CommonServices => _commonServices;

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        foreach (var postTagType in Enum.GetValues<PostTagType>())
        {
            for (var i = 0; i < 10; i++)
            {
                dbContext.BlizzardData.Add(new BlizzardDataRecord(postTagType, i));
            }
        }

        await dbContext.SaveChangesAsync();
    }

    protected async Task<AccountViewModel> CreateUser(Session session, string name, long id = 1, string battleTag = "None")
    {
        var user = new User("", name).WithIdentity($"Test:{_userId}");

        user = user.WithClaim("BattleNet-Id", id.ToString());
        user = user.WithClaim("BattleNet-Tag", battleTag);
        user = user.WithClaim("BattleNet-Token", "NULL TOKEN");
        user = user.WithClaim("BattleNet-TokenExpires", "0");

        await CommonServices.Commander.Call(new AuthBackend_SignIn(session, user));

        var account = await CommonServices.AccountServices.TryGetActiveAccount(session);
        account.Should().NotBeNull();
        account.Id.Should().Be(_userId);

        _userId++;

        return account;
    }

    private ServiceProvider GetServiceProvider()
    {
        var config = new CommonConfig
        {
            DatabaseConnectionString = null,
            UploadToBlobStorage = false,
        };

        if (config.DatabaseConnectionString != null)
        {
            throw new NotImplementedException();
        }

        var services = new ServiceCollection();
        var helper = new ProgramHeleprTests(config, services);
        helper.Initialize();

        var app = services.BuildServiceProvider();
        helper.Configure(app);

        return app;
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    protected AppDbContext CreateDbContext()
    {
        return _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
    }

    protected static byte[] GetImageData(int width, int height)
    {
        using Image<Rgba32> image = new(100, 100);
        using var memoryStream = new MemoryStream();
        image.SaveAsJpeg(memoryStream);

        return memoryStream.ToArray();
    }

    protected async Task CreateRandomCharacters(AccountViewModel accountViewModel, int count)
    {
        await using var database = CreateDbContext();
        for (var i = 0; i < count; i++)
        {
            var id = _characterId++;
            var name = $"Character:{id}";

            var moaRef = MoaRef.GetCharacterRef(BlizzardRegion.Europe, "none", name, id);
            var characterRecord = await CommonServices.CharacterServices.GetOrCreateCharacterRecord(moaRef.Full);

            database.Attach(characterRecord);
            characterRecord.AccountId = accountViewModel.Id;
            characterRecord.Name = name;
            characterRecord.RealmId = 1;
            characterRecord.Race = (byte)Random.Shared.Next(0, 10);
            characterRecord.Class = (byte)Random.Shared.Next(0, 10);
        }

        await database.SaveChangesAsync();

        using (Computed.Invalidate())
        {
            _ = CommonServices.AccountServices.DependsOnAccountRecord(accountViewModel.Id);
            _ = CommonServices.AccountServices.DependsOnAccountRecord(accountViewModel.Id);
            _ = CommonServices.AccountServices.DependsOnAccountAchievements(accountViewModel.Id);
            _ = CommonServices.CharacterServices.TryGetAllAccountCharacters(accountViewModel.Id);
        }
    }
}