﻿using AzerothMemories.WebBlazor.Components;

namespace AzerothMemories.WebBlazor.Pages;

public sealed class IndexPageViewModel : ViewModelBase
{
    public ActiveAccountViewModel AccountViewModel { get; private set; }

    public RecentPostsHelper RecentPostsHelper { get; private set; }

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        RecentPostsHelper = new RecentPostsHelper(Services);
    }

    public async Task ComputeState(string currentPageString, string sortModeString, string postTypeString)
    {
        AccountViewModel = await Services.AccountServices.TryGetAccount(null);

        await RecentPostsHelper.ComputeState(currentPageString, sortModeString, postTypeString);
    }
}