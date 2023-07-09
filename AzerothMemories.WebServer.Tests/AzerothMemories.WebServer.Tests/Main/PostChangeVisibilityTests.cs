using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostChangeVisibilityTests : BaseTestHelper
{
    [Fact]
    public async Task CanChangePostVisibility()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TrySetPostVisibility(session, validPost.PostId, 1));
        result.Should().Be(1);
    }

    [Fact]
    public async Task CanNotChangeOtherAccountsPostVisibility()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var session2 = Session.New();

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TrySetPostVisibility(session2, validPost.PostId, 1));
        result.Should().BeNull();
    }

    [Fact]
    public async Task CanNotChangeRandomPostVisibility()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TrySetPostVisibility(session, 999, 1));
        result.Should().BeNull();
    }
}