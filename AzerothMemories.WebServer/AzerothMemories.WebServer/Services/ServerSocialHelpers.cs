using System.Linq.Expressions;

namespace AzerothMemories.WebServer.Services
{
    public sealed class ServerSocialHelpers
    {
        public static readonly Func<AccountRecord, string>[] GetterFunc;
        public static readonly Action<AccountRecord, string>[] SetterFunc;
        public static readonly Expression<Func<AccountRecord, string>>[] QuerySetter;

        static ServerSocialHelpers()
        {
            GetterFunc = new Func<AccountRecord, string>[(int)SocialLinks.Count];
            SetterFunc = new Action<AccountRecord, string>[(int)SocialLinks.Count];
            QuerySetter = new Expression<Func<AccountRecord, string>>[(int)SocialLinks.Count];

            GetterFunc[(int)SocialLinks.Discord] = r => r.SocialDiscord;
            SetterFunc[(int)SocialLinks.Discord] = (r, s) => r.SocialDiscord = s;
            QuerySetter[(int)SocialLinks.Discord] = r => r.SocialDiscord;

            GetterFunc[(int)SocialLinks.Twitter] = r => r.SocialTwitter;
            SetterFunc[(int)SocialLinks.Twitter] = (r, s) => r.SocialTwitter = s;
            QuerySetter[(int)SocialLinks.Twitter] = r => r.SocialTwitter;

            GetterFunc[(int)SocialLinks.Twitter] = r => r.SocialTwitter;
            SetterFunc[(int)SocialLinks.Twitter] = (r, s) => r.SocialTwitter = s;
            QuerySetter[(int)SocialLinks.Twitter] = r => r.SocialTwitter;

            GetterFunc[(int)SocialLinks.Twitch] = r => r.SocialTwitch;
            SetterFunc[(int)SocialLinks.Twitch] = (r, s) => r.SocialTwitch = s;
            QuerySetter[(int)SocialLinks.Twitch] = r => r.SocialTwitch;

            GetterFunc[(int)SocialLinks.YouTube] = r => r.SocialYouTube;
            SetterFunc[(int)SocialLinks.YouTube] = (r, s) => r.SocialYouTube = s;
            QuerySetter[(int)SocialLinks.YouTube] = r => r.SocialYouTube;

            Exceptions.ThrowIf(GetterFunc.Any(x => x == null));
            Exceptions.ThrowIf(SetterFunc.Any(x => x == null));
            Exceptions.ThrowIf(QuerySetter.Any(x => x == null));
        }
    }
}