using AwesomeAssertions;
using AzerothMemories.WebServer.Tests.Common;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostCommentTests : BaseTestHelper
{
    [Fact]
    public async Task CanNotCommentWithoutAccount()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryPublishComment(Session.New(), validPost.PostId, 0, "Test Comment"), TestContext.Current.CancellationToken);
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotCommentOnInvalidPost()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPublishComment(session, 99, 0, "Test Comment"), TestContext.Current.CancellationToken);
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotCommentOnInvalidComment()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 99, "Test Comment"), TestContext.Current.CancellationToken);
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanCommentOnPost()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"), TestContext.Current.CancellationToken);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanCommentOnComment()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var postCommentResult = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"), TestContext.Current.CancellationToken);
        postCommentResult.Should().BeGreaterThan(0);

        var commentResult = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, postCommentResult, "Test Comment"), TestContext.Current.CancellationToken);
        commentResult.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanCommentOnCommentDepthTest()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var parentComment = 0;
        for (var i = 0; i < ZExtensions.MaxCommentDepth + 1; i++)
        {
            var postCommentResult = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, parentComment, "Test Comment"), TestContext.Current.CancellationToken);
            postCommentResult.Should().BeGreaterThan(0);

            parentComment = postCommentResult;
        }

        var commentResult = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, parentComment, "Test Comment"), TestContext.Current.CancellationToken);
        commentResult.Should().BeGreaterThan(0);
    }
}