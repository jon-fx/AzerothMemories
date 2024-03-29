﻿using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostCommentDeleteTests : BaseTestHelper
{
    [Fact]
    public async Task CanDeleteOwnComments()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryDeleteComment(session, validPost.PostId, validComment));
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CanNotDeleteOtherComments()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryDeleteComment(Session.New(), validPost.PostId, validComment));
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotDeleteRandomComments()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var result = await CommonServices.Commander.Call(new Post_TryDeleteComment(session, validPost.PostId, 99));
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotDeleteRandomComments2()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryDeleteComment(session, 99, 99));
        result.Should().Be(0);
    }

    [Fact]
    public async Task CanNotDeleteRandomComments3()
    {
        var session = Session.New();
        var account = await CreateUser(session, "Bob");

        var validPost = await PostCreateTests.CreateValidPost(CommonServices, session, account);
        var validComment = await CommonServices.Commander.Call(new Post_TryPublishComment(session, validPost.PostId, 0, "Test Comment"));
        validComment.Should().BeGreaterThan(0);

        var result = await CommonServices.Commander.Call(new Post_TryDeleteComment(session, 99, validComment));
        result.Should().Be(0);
    }
}