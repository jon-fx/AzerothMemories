using AwesomeAssertions;
using AzerothMemories.WebServer.Tests.Common;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class AccountBattleTagTests : BaseTestHelper
{
    [Fact]
    public async Task BattleTagIsNotPublic()
    {
        var session1 = Session.New();
        var battleTag = "TestBattleTag123";
        var account1 = await CreateUser(session1, "Bob", 1, battleTag);

        account1.Should().NotBeNull();
        account1.Username.Should().Be($"User-{account1.Id}");
        account1.BattleTagIsPublic.Should().BeFalse();

        var account = await CommonServices.AccountServices.TryGetAccountById(session1, account1.Id);

        account = account.ThrowIfNull();
        account.BattleTag.Should().BeNull();
    }
}