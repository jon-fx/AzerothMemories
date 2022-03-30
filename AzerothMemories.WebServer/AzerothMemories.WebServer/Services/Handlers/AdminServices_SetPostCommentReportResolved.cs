﻿namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AdminServices_SetPostCommentReportResolved
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Admin_SetPostCommentReportResolved command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invalidateReports = context.Operation().Items.Get<Admin_InvalidateReports>();
            if (invalidateReports != null)
            {
                _ = commonServices.PostServices.DependsOnPostCommentReports();
            }

            return default;
        }

        var account = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return false;
        }

        var result = false;
        if (command.Delete)
        {
            var deletedTime = await commonServices.PostServices.TryDeleteComment(new Post_TryDeleteComment(command.Session, command.PostId, command.CommentId)).ConfigureAwait(false);
            result = deletedTime != 0;
        }
        else
        {
            await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);

            var reports = await database.PostCommentReports.Where(x => x.CommentId == command.CommentId).ToArrayAsync().ConfigureAwait(false);
            foreach (var report in reports)
            {
                report.ResolvedByAccountId = account.Id;
            }

            await database.SaveChangesAsync().ConfigureAwait(false);
        }

        //if (result)
        {
            context.Operation().Items.Set(new Admin_InvalidateReports(true));
        }

        return result;
    }
}