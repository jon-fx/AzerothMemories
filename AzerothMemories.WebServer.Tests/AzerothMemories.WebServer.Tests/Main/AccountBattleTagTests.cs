using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class AccountBattleTagTests : BaseTestHelper
{
    [Fact]
    public async Task BattleTagIsNotPublic()
    {
        var session1 = SessionFactory.CreateSession();
        var battleTag = "TestBattleTag123";
        var account1 = await CreateUser(session1, "Bob", 1, battleTag);

        account1.Should().NotBeNull();
        account1.Username.Should().Be($"User-{account1.Id}");
        //account1.BattleTag.Should().Be(battleTag);
        account1.BattleTagIsPublic.Should().BeFalse();

        var account = await CommonServices.AccountServices.TryGetAccountById(Session.Default, account1.Id);
        account.BattleTag.Should().BeNull();
    }

    //[Fact]
    //public async Task BattleTagIsPublic()
    //{
    //    var session1 = SessionFactory.CreateSession();
    //    var battleTag = "TestBattleTag123";
    //    var account1 = await CreateUser(session1, "Bob", 1, battleTag);

    //    account1.Should().NotBeNull();
    //    account1.Username.Should().Be($"User-{account1.Id}");
    //    account1.BattleTag.Should().Be(battleTag);
    //    account1.BattleTagIsPublic.Should().BeFalse();

    //    var newValue = await CommonServices.AccountServices.TryChangeBattleTagVisibility(new Account_TryChangeBattleTagVisibility(session1, true));
    //    newValue.Should().BeTrue();

    //    var account = await CommonServices.AccountServices.TryGetAccountById(Session.Default, account1.Id);
    //    account.BattleTag.Should().Be(battleTag);
    //}
}