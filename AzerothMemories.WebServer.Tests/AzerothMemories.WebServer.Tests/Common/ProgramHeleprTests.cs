using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AzerothMemories.WebServer.Tests.Common;

internal sealed class ProgramHeleprTests : ProgramHelper
{
    public ProgramHeleprTests(CommonConfig config, IServiceCollection services) : base(config, services)
    {
    }

    protected override void ConfigureDbContextFactory(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Database").ConfigureWarnings(warnings =>
        {
            warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
        });
    }

    protected override void OnInitializeAuth()
    {
    }
}