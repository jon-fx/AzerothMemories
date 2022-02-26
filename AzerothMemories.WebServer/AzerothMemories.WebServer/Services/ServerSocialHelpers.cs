namespace AzerothMemories.WebServer.Services;

public static class ServerSocialHelpers
{
    public static readonly Func<AccountRecord, string>[] GetterFunc;
    public static readonly Action<AccountRecord, string>[] SetterFunc;
    public static readonly Action<AccountRecord, string>[] QuerySetter;

    static ServerSocialHelpers()
    {
        GetterFunc = new Func<AccountRecord, string>[(int)SocialLinks.Count];
        SetterFunc = new Action<AccountRecord, string>[(int)SocialLinks.Count];
        QuerySetter = new Action<AccountRecord, string>[(int)SocialLinks.Count];

        GetterFunc[(int)SocialLinks.Discord] = r => r.SocialDiscord;
        SetterFunc[(int)SocialLinks.Discord] = (r, s) => r.SocialDiscord = s;
        QuerySetter[(int)SocialLinks.Discord] = (a, v) => a.SocialDiscord = v;

        GetterFunc[(int)SocialLinks.Twitter] = r => r.SocialTwitter;
        SetterFunc[(int)SocialLinks.Twitter] = (r, s) => r.SocialTwitter = s;
        QuerySetter[(int)SocialLinks.Twitter] = (a, v) => a.SocialTwitter = v;

        GetterFunc[(int)SocialLinks.Twitch] = r => r.SocialTwitch;
        SetterFunc[(int)SocialLinks.Twitch] = (r, s) => r.SocialTwitch = s;
        QuerySetter[(int)SocialLinks.Twitch] = (a, v) => a.SocialTwitch = v;

        GetterFunc[(int)SocialLinks.YouTube] = r => r.SocialYouTube;
        SetterFunc[(int)SocialLinks.YouTube] = (r, s) => r.SocialYouTube = s;
        QuerySetter[(int)SocialLinks.YouTube] = (a, v) => a.SocialYouTube = v;

        Exceptions.ThrowIf(GetterFunc.Any(x => x == null));
        Exceptions.ThrowIf(SetterFunc.Any(x => x == null));
        Exceptions.ThrowIf(QuerySetter.Any(x => x == null));
    }
}