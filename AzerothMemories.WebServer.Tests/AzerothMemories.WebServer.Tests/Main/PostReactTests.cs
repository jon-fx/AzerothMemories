using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostReactTests : BaseTestHelper
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
    public async Task CanReactToPost(PostReaction reaction)
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPost(session, validPost.PostId, reaction));
        result.Should().BeGreaterThan(0);

        await EnsureReactions(session, account, validPost, reaction, result);
    }

    [Fact]
    public async Task ReactWithoutAccountDoesNothing()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPost(Session.New(), validPost.PostId, PostReaction.Reaction1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactNoneDoesNothing()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPost(session, validPost.PostId, PostReaction.None));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactWithInvliadReaction()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        var result = await CommonServices.Commander.Call(new Post_TryReactToPost(session, validPost.PostId, (PostReaction)25));
        result.Should().Be(0);
    }

    [Fact]
    public async Task ReactWithInvliadPost()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryReactToPost(session, 99, PostReaction.Reaction1));
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
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);

        for (var i = 1; i < (int)(PostReaction.Reaction9 + 1); i++)
        {
            var result = await CommonServices.Commander.Call(new Post_TryReactToPost(session, validPost.PostId, reaction));
            result.Should().BeGreaterThan(0);

            await EnsureReactions(session, account, validPost, reaction, result);

            var result2 = await CommonServices.Commander.Call(new Post_TryReactToPost(session, validPost.PostId, (PostReaction)i));
            result2.Should().Be(result);

            await EnsureReactions(session, account, validPost, (PostReaction)i, result2);
        }
    }

    private async Task EnsureReactions(Session session, AccountViewModel account, AddMemoryResult validPost, PostReaction reaction, int reactionId)
    {
        var allPostReaction = await CommonServices.PostServices.TryGetPostReactions(validPost.PostId);
        var containsResult = allPostReaction.TryGetValue(account.Id, out var reactionViewModel);
        containsResult.Should().BeTrue();

        reactionViewModel.Should().NotBeNull();
        reactionViewModel?.Id.Should().Be(reactionId);
        reactionViewModel?.Reaction.Should().Be(reaction);

        var postViewModel = await CommonServices.PostServices.TryGetPostViewModel(account.Id, validPost.PostId, ServerSideLocale.En_Gb);
        postViewModel.Should().NotBeNull();
        postViewModel.Id.Should().Be(validPost.PostId);
        postViewModel.AccountId.Should().Be(validPost.AccountId);
        postViewModel.Reaction.Should().Be(reaction);
        postViewModel.ReactionId.Should().Be(reactionId);

        for (var i = 0; i < postViewModel.ReactionCounters.Length; i++)
        {
            if (i == (int)reaction - 1)
            {
                postViewModel.ReactionCounters[i].Should().Be(1);
            }
            else
            {
                postViewModel.ReactionCounters[i].Should().Be(0);
            }
        }

        postViewModel.TotalReactionCount.Should().Be(1);
    }
}