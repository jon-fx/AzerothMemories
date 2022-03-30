﻿namespace AzerothMemories.WebServer.Services.Handlers;

internal static class AdminServices_SetPostTagReportResolved
{
    public static async Task<bool> TryHandle(CommonServices commonServices, IDatabaseContextProvider databaseContextProvider, Admin_SetPostTagReportResolved command)
    {
        var context = CommandContext.GetCurrent();
        if (Computed.IsInvalidating())
        {
            var invalidateReports = context.Operation().Items.Get<Admin_InvalidateReports>();
            if (invalidateReports != null)
            {
                _ = commonServices.PostServices.DependsOnPostTagReports();
            }

            return default;
        }

        var account = await commonServices.AccountServices.TryGetActiveAccount(command.Session).ConfigureAwait(false);
        if (!account.IsAdmin())
        {
            return false;
        }

        if (command.Delete)
        {
            var postTags = await commonServices.PostServices.GetAllPostTags(command.PostId).ConfigureAwait(false);
            if (postTags.Length == 0)
            {
                return false;
            }

            var postTagsStrings = postTags.ToDictionary(x => x.TagString, x => x);
            if (!postTagsStrings.Remove(command.TagString))
            {
                return false;
            }

            var newTagStrings = postTagsStrings.Keys.ToHashSet();
            var updateSystemTagResult = await commonServices.PostServices.TryUpdateSystemTags(new Post_TryUpdateSystemTags(command.Session, command.PostId, Post_TryUpdateSystemTags.DefaultAvatar, newTagStrings)).ConfigureAwait(false);
            if (updateSystemTagResult == AddMemoryResultCode.Success)
            {
            }
        }
        else
        {
            await using var database = await databaseContextProvider.CreateCommandDbContext().ConfigureAwait(false);

            var reports = await database.PostTagReports.Where(x => x.TagId == command.ReportedTagId).ToArrayAsync().ConfigureAwait(false);
            foreach (var report in reports)
            {
                report.ResolvedByAccountId = account.Id;
            }

            await database.SaveChangesAsync().ConfigureAwait(false);
        }

        context.Operation().Items.Set(new Admin_InvalidateReports(true));

        return false;
    }
}