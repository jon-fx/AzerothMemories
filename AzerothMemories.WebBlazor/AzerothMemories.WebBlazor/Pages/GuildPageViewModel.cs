﻿namespace AzerothMemories.WebBlazor.Pages;

public sealed class GuildPageViewModel : ViewModelBase
{
    public string ErrorMessage { get; private set; }

    public GuildViewModel GuildViewModel { get; private set; }

    public PostSearchHelper PostSearchHelper { get; private set; }

    public bool IsLoading => GuildViewModel == null || PostSearchHelper == null;

    public override async Task OnInitialized()
    {
        await base.OnInitialized();

        PostSearchHelper = new PostSearchHelper(Services);
    }

    public async Task ComputeState(long id, string region, string realm, string name, string sortModeString, string currentPageString)
    {
        GuildViewModel guildViewModel;
        if (id > 0)
        {
            guildViewModel = await Services.GuildServices.TryGetGuild(null, id);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                ErrorMessage = "Invalid Region";
                return;
            }

            if (string.IsNullOrWhiteSpace(realm))
            {
                ErrorMessage = "Invalid Realm";
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorMessage = "Invalid Name";
                return;
            }

            var regionInfo = BlizzardRegionInfo.AllByName.Values.FirstOrDefault(x => x.TwoLetters.ToLowerInvariant() == region);
            if (regionInfo == null)
            {
                ErrorMessage = "Invalid Region";
                return;
            }

            if (!Services.TagHelpers.GetRealmId(realm, out _))
            {
                ErrorMessage = "Invalid Realm";
                return;
            }

            guildViewModel = await Services.GuildServices.TryGetGuild(null, regionInfo.Region, realm, name);
        }

        if (guildViewModel == null)
        {
            ErrorMessage = "Invalid Guild";
            return;
        }

        GuildViewModel = guildViewModel;

        var guildTag = new PostTagInfo(PostTagType.Guild, GuildViewModel.Id, GuildViewModel.Name, null);// GuildViewModel.AvatarLinkWithFallBack);
        await PostSearchHelper.ComputeState(new[] { guildTag.TagString }, sortModeString, currentPageString, null, null);
    }
}