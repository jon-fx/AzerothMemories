﻿namespace AzerothMemories.WebBlazor.Services;

[BasePath("post")]
public interface IPostServices
{
    [Post(nameof(TryPostMemory))]
    Task<AddMemoryResult> TryPostMemory(Session session, [Body] byte[] toArray);

    [ComputeMethod]
    [Get(nameof(TryGetPostViewModel) + "/{accountId}/{postId}")]
    Task<PostViewModel> TryGetPostViewModel(Session session, [Path] long accountId, [Path] long postId, [Query] string locale);

    [CommandHandler]
    [Post(nameof(TryReactToPost))]
    Task<long> TryReactToPost([Body] Post_TryReactToPost command, CancellationToken cancellationToken = default);

    [ComputeMethod]
    [Get(nameof(TryGetReactions) + "/{postId}")]
    Task<PostReactionViewModel[]> TryGetReactions(Session session, [Path] long postId);

    [ComputeMethod]
    [Get(nameof(TryGetCommentsPage) + "/{postId}")]
    Task<PostCommentPageViewModel> TryGetCommentsPage(Session session, [Path] long postId, [Query] int page = 0, [Query] long focusedCommentId = 0);

    [ComputeMethod]
    [Get(nameof(TryGetCommentReactionData) + "/{postId}/{commentId}")]
    Task<PostReactionViewModel[]> TryGetCommentReactionData(Session session, [Path] long postId, [Path] long commentId);

    [ComputeMethod]
    [Get(nameof(TryGetMyCommentReactions) + "/{postId}")]
    Task<Dictionary<long, PostCommentReactionViewModel>> TryGetMyCommentReactions(Session session, [Path] long postId);

    [CommandHandler]
    [Post(nameof(TryRestoreMemory))]
    Task<bool> TryRestoreMemory([Body] Post_TryRestoreMemory command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryPublishComment))]
    Task<long> TryPublishComment([Body] Post_TryPublishComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryReactToPostComment))]
    Task<long> TryReactToPostComment([Body] Post_TryReactToPostComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TrySetPostVisibility))]
    Task<byte?> TrySetPostVisibility([Body] Post_TrySetPostVisibility command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryDeletePost))]
    Task<long> TryDeletePost([Body] Post_TryDeletePost command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryDeleteComment))]
    Task<long> TryDeleteComment([Body] Post_TryDeleteComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryReportPost))]
    Task<bool> TryReportPost([Body] Post_TryReportPost command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryReportPostComment))]
    Task<bool> TryReportPostComment([Body] Post_TryReportPostComment command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryReportPostTags))]
    Task<bool> TryReportPostTags([Body] Post_TryReportPostTags command, CancellationToken cancellationToken = default);

    [CommandHandler]
    [Post(nameof(TryUpdateSystemTags))]
    Task<AddMemoryResultCode> TryUpdateSystemTags([Body] Post_TryUpdateSystemTags command, CancellationToken cancellationToken = default);
}