using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public class AccountUsernameTests : BaseTestHelper
{
    [Fact]
    public async Task SetsDefaultUsername()
    {
        var session1 = Session.New();
        var account1 = await CreateUser(session1, "Bob");

        account1.Should().NotBeNull();
        account1.Username.Should().Be($"User-{account1.Id}");
    }

    [Theory]
    [InlineData("Bob")]
    [InlineData("Bill")]
    [InlineData("Dragon")]
    public async Task CanChangeUsername(string username)
    {
        var session1 = Session.New();
        var account1 = await CreateUser(session1, "Bob");

        account1.Should().NotBeNull();
        account1.Username.Should().Be($"User-{account1.Id}");

        var result = await CommonServices.Commander.Call(new Account_TryChangeUsername(session1, 0, username));
        result.Should().BeTrue();

        var accountRecord = await CommonServices.AccountServices.TryGetActiveAccount(session1);
        accountRecord.Id.Should().Be(account1.Id);
        accountRecord.Username.Should().Be(username);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("1Number")]
    [InlineData("Exclamation!")]
    [InlineData("Dollar$")]
    [InlineData("Underscore_")]
    [InlineData("Dash-")]
    [InlineData("AVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongUsername")]
    public async Task CanNotChangeUsername(string username)
    {
        var session1 = Session.New();
        var account1 = await CreateUser(session1, "Bob");

        account1.Should().NotBeNull();
        account1.Username.Should().Be($"User-{account1.Id}");

        var result = await CommonServices.Commander.Call(new Account_TryChangeUsername(session1, 0, username));
        result.Should().BeFalse();

        var accountRecord = await CommonServices.AccountServices.TryGetActiveAccount(session1);
        accountRecord.Id.Should().Be(account1.Id);
        accountRecord.Username.Should().Be(account1.Username);
    }

    [Fact]
    public async Task CanNotUseSameUsername()
    {
        var session1 = Session.New();
        var account1 = await CreateUser(session1, "Bob");
        var result1 = await CommonServices.Commander.Call(new Account_TryChangeUsername(session1, 0, "Bob"));
        result1.Should().BeTrue();

        var session2 = Session.New();
        var account2 = await CreateUser(session2, "Bob");
        var result2 = await CommonServices.Commander.Call(new Account_TryChangeUsername(session2, 0, "Bob"));
        result2.Should().BeFalse();
    }
}