namespace AzerothMemories.WebServer.Services;

public static class ServerSocialHelpers
{
    public static readonly Func<AccountRecord, string>[] GetterFunc;
    public static readonly Action<AccountRecord, string>[] SetterFunc;

    static ServerSocialHelpers()
    {
        GetterFunc = new Func<AccountRecord, string>[(int)SocialLinks.Count];
        SetterFunc = new Action<AccountRecord, string>[(int)SocialLinks.Count];

        GetterFunc[(int)SocialLinks.Discord] = r => r.SocialDiscord;
        SetterFunc[(int)SocialLinks.Discord] = (r, s) => r.SocialDiscord = s;

        GetterFunc[(int)SocialLinks.Twitter] = r => r.SocialTwitter;
        SetterFunc[(int)SocialLinks.Twitter] = (r, s) => r.SocialTwitter = s;

        GetterFunc[(int)SocialLinks.Twitch] = r => r.SocialTwitch;
        SetterFunc[(int)SocialLinks.Twitch] = (r, s) => r.SocialTwitch = s;

        GetterFunc[(int)SocialLinks.YouTube] = r => r.SocialYouTube;
        SetterFunc[(int)SocialLinks.YouTube] = (r, s) => r.SocialYouTube = s;

        Exceptions.ThrowIf(GetterFunc.Any(x => x == null));
        Exceptions.ThrowIf(SetterFunc.Any(x => x == null));
    }
}