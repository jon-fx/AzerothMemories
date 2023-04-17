using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class FollowingTests : BaseTestHelper
{
    [Fact]
    public async Task CanNotFollowWithNoSession()
    {
        var session1 = SessionFactory.CreateSession();
        //var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanNotFollowSameAccount()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account1.Id));
        startFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanNotFollowInvalidAccount()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, 99));
        startFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanFollowWhenAccountIsNotPrivate()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Active);
    }

    [Fact]
    public async Task CanFollowIsPendingWhenAccountIsPrivate()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        await CommonServices.Commander.Call(new Account_TryChangeIsPrivate(session2, true));

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Pending);
    }

    [Fact]
    public async Task CanNotAcceptWithNoSession()
    {
        var session1 = SessionFactory.CreateSession();
        //var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryAcceptFollower(session1, account2.Id));
        startFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanNotAcceptSameAccount()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryAcceptFollower(session1, account1.Id));
        startFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanNotAcceptFollowingWhenNotPending()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var acceptFollowing = await CommonServices.Commander.Call(new Following_TryAcceptFollower(session2, account1.Id));
        acceptFollowing.Should().Be(null);
    }

    [Fact]
    public async Task CanAcceptFollowingWhenAccountIsPrivate()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        await CommonServices.Commander.Call(new Account_TryChangeIsPrivate(session2, true));

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Pending);

        var acceptFollowing = await CommonServices.Commander.Call(new Following_TryAcceptFollower(session2, account1.Id));
        acceptFollowing.Should().Be(AccountFollowingStatus.Active);
    }

    [Fact]
    public async Task CanStopFollowing()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Active);

        var stopFollowing = await CommonServices.Commander.Call(new Following_TryStopFollowing(session1, account2.Id));
        stopFollowing.Should().Be(AccountFollowingStatus.None);
    }

    [Fact]
    public async Task CanStopFollowingFails()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Active);

        var stopFollowing = await CommonServices.Commander.Call(new Following_TryStopFollowing(session2, account1.Id));
        stopFollowing.Should().BeNull();
    }

    [Fact]
    public async Task CanRemoveFollower()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Active);

        var stopFollowing = await CommonServices.Commander.Call(new Following_TryRemoveFollower(session2, account1.Id));
        stopFollowing.Should().Be(AccountFollowingStatus.None);
    }

    [Fact]
    public async Task CanRemoveFollowerFails()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        var session2 = SessionFactory.CreateSession();
        var account2 = await CreateUser(session2, "Bill");

        var startFollowing = await CommonServices.Commander.Call(new Following_TryStartFollowing(session1, account2.Id));
        startFollowing.Should().Be(AccountFollowingStatus.Active);

        var stopFollowing = await CommonServices.Commander.Call(new Following_TryRemoveFollower(session1, account2.Id));
        stopFollowing.Should().Be(null);
    }
}