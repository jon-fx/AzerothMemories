using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostDeleteTests : BaseTestHelper
{
    [Fact]
    public async Task CanDeleteOwnPost()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryDeletePost(session, validPost.PostId));
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanNotDeleteOtherPost()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryDeletePost(SessionFactory.CreateSession(), validPost.PostId));
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotDeleteNonePost()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryDeletePost(session, 99));
        result.Should().Be(0);
    }
}