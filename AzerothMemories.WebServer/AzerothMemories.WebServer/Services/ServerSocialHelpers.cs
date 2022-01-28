namespace AzerothMemories.WebServer.Services;

public static class ServerSocialHelpers
{
    public static readonly Func<AccountRecord, string>[] GetterFunc;
    public static readonly Action<AccountRecord, string>[] SetterFunc;
    public static readonly Func<AppDbContext, long, string, Task<int>>[] QuerySetter;

    static ServerSocialHelpers()
    {
        GetterFunc = new Func<AccountRecord, string>[(int)SocialLinks.Count];
        SetterFunc = new Action<AccountRecord, string>[(int)SocialLinks.Count];
        QuerySetter = new Func<AppDbContext, long, string, Task<int>>[(int)SocialLinks.Count];

        GetterFunc[(int)SocialLinks.Discord] = r => r.SocialDiscord;
        SetterFunc[(int)SocialLinks.Discord] = (r, s) => r.SocialDiscord = s;
        QuerySetter[(int)SocialLinks.Discord] = (d, i, v) => d.Accounts.Where(x => x.Id == i).UpdateAsync(x => new AccountRecord { SocialDiscord = v });

        GetterFunc[(int)SocialLinks.Twitter] = r => r.SocialTwitter;
        SetterFunc[(int)SocialLinks.Twitter] = (r, s) => r.SocialTwitter = s;
        QuerySetter[(int)SocialLinks.Twitter] = (d, i, v) => d.Accounts.Where(x => x.Id == i).UpdateAsync(x => new AccountRecord { SocialTwitter = v });

        GetterFunc[(int)SocialLinks.Twitch] = r => r.SocialTwitch;
        SetterFunc[(int)SocialLinks.Twitch] = (r, s) => r.SocialTwitch = s;
        QuerySetter[(int)SocialLinks.Twitch] = (d, i, v) => d.Accounts.Where(x => x.Id == i).UpdateAsync(x => new AccountRecord { SocialTwitch = v });

        GetterFunc[(int)SocialLinks.YouTube] = r => r.SocialYouTube;
        SetterFunc[(int)SocialLinks.YouTube] = (r, s) => r.SocialYouTube = s;
        QuerySetter[(int)SocialLinks.YouTube] = (d, i, v) => d.Accounts.Where(x => x.Id == i).UpdateAsync(x => new AccountRecord { SocialYouTube = v });

        Exceptions.ThrowIf(GetterFunc.Any(x => x == null));
        Exceptions.ThrowIf(SetterFunc.Any(x => x == null));
        Exceptions.ThrowIf(QuerySetter.Any(x => x == null));
    }
}