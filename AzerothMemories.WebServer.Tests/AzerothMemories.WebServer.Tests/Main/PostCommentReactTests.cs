using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostCommentReactTests : BaseTestHelper
{
    [Theory]
    [InlineData(PostReaction.Reaction1)]
    [InlineData(PostReaction.Reaction2)]
    [InlineData(PostReaction.Reaction3)]
    [InlineData(PostReaction.Reaction4)]
    [InlineData(PostReaction.Reaction5)]
    [InlineData(PostReaction.Reaction6)]
    [InlineData(PostReaction.Reaction7)]
    [InlineData(PostReaction.Reaction8)]
    [InlineData(PostReaction.Reaction9)]
    public async Task CanReactToComment(PostReaction reaction)
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, validComment, reaction));
        result.Should().BeGreaterThan(0);

        await EnsureReactions(session, account, validPost, validComment, reaction, result);
    }

    [Fact]
    public async Task ReactWithoutAccountDoesNothing()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(SessionFactory.CreateSession(), validPost.PostId, validComment, PostReaction.Reaction1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactWithInvliadPost()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, 99, 99, PostReaction.Reaction1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactWithInvliadComment()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, 99, PostReaction.Reaction1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactNoneDoesNothing()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, validComment, PostReaction.None));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactWithInvliadReaction()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, validComment, (PostReaction)25));
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(PostReaction.Reaction1)]
    [InlineData(PostReaction.Reaction2)]
    [InlineData(PostReaction.Reaction3)]
    [InlineData(PostReaction.Reaction4)]
    [InlineData(PostReaction.Reaction5)]
    [InlineData(PostReaction.Reaction6)]
    [InlineData(PostReaction.Reaction7)]
    [InlineData(PostReaction.Reaction8)]
    [InlineData(PostReaction.Reaction9)]
    public async Task ReactCanChange(PostReaction reaction)
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        for (var i = 1; i < (int)(PostReaction.Reaction9 + 1); i++)
        {
            var result = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, validComment, reaction));
            result.Should().BeGreaterThan(0);

            await EnsureReactions(session, account, validPost, validComment, reaction, result);

            var result2 = await CommonServices.Commander.Call(new Post_TryReactToPostComment(session, validPost.PostId, validComment, (PostReaction)i));
            result2.Should().Be(result);

            await EnsureReactions(session, account, validPost, validComment, (PostReaction)i, result2);
        }
    }

    private async Task EnsureReactions(Session session, AccountViewModel account, AddMemoryResult validPost, int validComment, PostReaction reaction, int reactionId)
    {
        var allPostReaction = await CommonServices.PostServices.TryGetMyCommentReactions(session, validPost.PostId);
        var containsResult = allPostReaction.TryGetValue(validComment, out var reactionViewModel);
        containsResult.Should().BeTrue();

        reactionViewModel.Should().NotBeNull();
        reactionViewModel?.Id.Should().Be(reactionId);
        reactionViewModel?.Reaction.Should().Be(reaction);

        await using var database = CreateDbContext();
        var commentViewModel = await database.PostComments.FirstOrDefaultAsync(x => x.Id == reactionId);
        commentViewModel.Should().NotBeNull();

        var reactionCounters = new[]
        {
            commentViewModel.ReactionCount1,
            commentViewModel.ReactionCount2,
            commentViewModel.ReactionCount3,
            commentViewModel.ReactionCount4,
            commentViewModel.ReactionCount5,
            commentViewModel.ReactionCount6,
            commentViewModel.ReactionCount7,
            commentViewModel.ReactionCount8,
            commentViewModel.ReactionCount9
        };

        for (var i = 0; i < reactionCounters.Length; i++)
        {
            if (i == (int)reaction - 1)
            {
                reactionCounters[i].Should().Be(1);
            }
            else
            {
                reactionCounters[i].Should().Be(0);
            }
        }

        commentViewModel.TotalReactionCount.Should().Be(1);
    }
}