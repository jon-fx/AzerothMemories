using System.Text;

namespace AzerothMemories.WebBlazor.Common;

public static class ZExtensions
{
    public static readonly string DefaultSocialImage = "header-banner.png";
    public static readonly string BlobStaticMediaStoragePath = $"media/{BlobStaticMedia}/";
    public static readonly string BlobUserUploadsStoragePath = $"media/{BlobUserUploads}/";
    public static readonly string BlobUserAvatarsStoragePath = $"media/{BlobUserAvatars}/";

    public static readonly List<string> ValidUploadExtensions = new() { ".gif", ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".png", ".bmp" };
    public static readonly string UploadAcceptExtensions = "image/*";

    public static readonly int MaxCommentDepth = 5;
    public static readonly int MaxPostCommentLength = 2048;
    public static readonly int MaxCommentLength = 2048;
    public static readonly int ReportPostCommentMaxLength = 200;
    public static readonly int MaxPostScreenShots = 8;
    public static readonly int MaxAddMemoryFileSizeInBytes = (int)10.Megabytes().Bytes;
    public static readonly int MaxAvatarFileSizeInBytes = (int)5.Megabytes().Bytes;

    public const string BlobStaticMedia = "media";
    public const string BlobUserUploads = "images";
    public const string BlobUserAvatars = "avatars";

    public const string AvatarBlobFilePrefix = "UserAvatar-";
    public static readonly string CustomUserAvatarPathPrefix = $"{BlobUserAvatarsStoragePath}{AvatarBlobFilePrefix}";

    public static readonly (int Min, int Max)[] TagCountsPerPost;
    public static readonly Instant MinPostTime = Instant.FromUnixTimeMilliseconds(946684800000);//Sat Jan 01 2000 00:00:00 GMT+0000

    static ZExtensions()
    {
        TagCountsPerPost = new (int Min, int Max)[(int)PostTagType.CountExcludingHashTag];

        TagCountsPerPost[(int)PostTagType.Type] = (1, 1);
        TagCountsPerPost[(int)PostTagType.Main] = (0, 5);

        TagCountsPerPost[(int)PostTagType.Region] = (1, 1);
        TagCountsPerPost[(int)PostTagType.Realm] = (0, 5);

        TagCountsPerPost[(int)PostTagType.Account] = (1, 50);
        TagCountsPerPost[(int)PostTagType.Character] = (0, 50);
        TagCountsPerPost[(int)PostTagType.Guild] = (0, 10);

        TagCountsPerPost[(int)PostTagType.Achievement] = (0, 10);
        TagCountsPerPost[(int)PostTagType.Item] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Mount] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Pet] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Zone] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Npc] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Spell] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Object] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Quest] = (0, 5);
        TagCountsPerPost[(int)PostTagType.ItemSet] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Toy] = (0, 5);
        TagCountsPerPost[(int)PostTagType.Title] = (0, 5);

        TagCountsPerPost[(int)PostTagType.CharacterRace] = (0, 5);
        TagCountsPerPost[(int)PostTagType.CharacterClass] = (0, 5);
        TagCountsPerPost[(int)PostTagType.CharacterClassSpecialization] = (0, 5);
    }

    public static bool IsRetailOnlyTag(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.Realm:
            case PostTagType.Character:
            case PostTagType.Guild:
            {
                return true;
            }
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Region:
            case PostTagType.Account:
            case PostTagType.Achievement:
            case PostTagType.Item:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Zone:
            case PostTagType.Npc:
            case PostTagType.Spell:
            case PostTagType.Object:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.HashTag:
            default:
            {
                return false;
            }
        }
    }

    public static bool CanBeReported(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.None:
            case PostTagType.HashTag:
            {
                return false;
            }
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Region:
            case PostTagType.Realm:
            case PostTagType.Account:
            case PostTagType.Character:
            case PostTagType.Guild:
            case PostTagType.Achievement:
            case PostTagType.Item:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Zone:
            case PostTagType.Npc:
            case PostTagType.Spell:
            case PostTagType.Object:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            {
                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    public static bool ParseTagInfoFrom(string key, out (PostTagType Type, int Id, string Text) result)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            result = default;
            return false;
        }

        var split = key.Split('-');

        PostTagType type;
        if (int.TryParse(split[0], out var typeAsInt))
        {
            if (Enum.IsDefined(typeof(PostTagType), typeAsInt))
            {
                type = (PostTagType)typeAsInt;
            }
            else
            {
                result = default;
                return false;
            }
        }
        else if (!Enum.TryParse(split[0], true, out type))
        {
            result = default;
            return false;
        }

        int id = -1;
        if (type == PostTagType.HashTag)
        {
        }
        else if (split.Length < 2 || !int.TryParse(split[1], out id))
        {
            result = default;
            return false;
        }

        result = (type, id, split[1]);

        return true;
    }

    public static string GetTagIcon(this PostTagType tagType)
    {
        switch (tagType)
        {
            case PostTagType.Zone:
            {
                return "🏰 ";
            }
            case PostTagType.Region:
            {
                return "🌌 ";
            }
            case PostTagType.Realm:
            {
                return "🪐 ";
            }
            case PostTagType.Account:
            case PostTagType.Character:
            {
                break;
            }
            case PostTagType.Guild:
            {
                return "🔶 ";
            }
            case PostTagType.HashTag:
            {
                return "📎 ";
            }
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            case PostTagType.Mount:
            case PostTagType.Pet:
            case PostTagType.Item:
            case PostTagType.Achievement:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.Npc:
            case PostTagType.Object:
            case PostTagType.Spell:
            case PostTagType.Quest:
            case PostTagType.ItemSet:
            case PostTagType.Toy:
            case PostTagType.Title:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return "";
    }

    public static string GetWowHeadLink(this PostTagType tagType, int id)
    {
        string wowHeadLink = null;
        switch (tagType)
        {
            case PostTagType.None:
            case PostTagType.Type:
            case PostTagType.Main:
            {
                break;
            }
            case PostTagType.Mount:
            {
                wowHeadLink = $"mount={id}";
                break;
            }
            case PostTagType.Pet:
            {
                wowHeadLink = $"battle-pet={id}";
                break;
            }
            case PostTagType.Item:
            {
                wowHeadLink = $"item={id}";
                break;
            }
            case PostTagType.Achievement:
            {
                wowHeadLink = $"achievement={id}";
                break;
            }
            case PostTagType.Zone:
            {
                wowHeadLink = $"zone={id}";
                break;
            }
            case PostTagType.Npc:
            {
                wowHeadLink = $"npc={id}";
                break;
            }
            case PostTagType.Spell:
            {
                wowHeadLink = $"spell={id}";
                break;
            }
            case PostTagType.Object:
            {
                wowHeadLink = $"object={id}";
                break;
            }
            case PostTagType.Quest:
            {
                wowHeadLink = $"quest={id}";
                break;
            }
            case PostTagType.ItemSet:
            {
                wowHeadLink = $"itemset={id}";
                break;
            }
            case PostTagType.Toy:
            case PostTagType.Title:
            case PostTagType.Region:
            case PostTagType.Realm:
            case PostTagType.Account:
            case PostTagType.Character:
            case PostTagType.Guild:
            case PostTagType.CharacterRace:
            case PostTagType.CharacterClass:
            case PostTagType.CharacterClassSpecialization:
            case PostTagType.HashTag:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        if (!string.IsNullOrWhiteSpace(wowHeadLink))
        {
            wowHeadLink += ServerSideLocaleExt.GetWowHeadDomain();
        }

        return wowHeadLink;
    }

    public static Color GetChipColor(this PostTagInfo tagInfo)
    {
        return Color.Primary;
    }

    public static string BuildReactionString(int totalReactionCount, int[] reactionCounters)
    {
        if (totalReactionCount == 0)
        {
            return string.Empty;
        }

        var added = false;
        var commentStr = new StringBuilder();
        commentStr.Append('(');

        for (var i = 1; i < (int)PostReaction.Reaction7; i++)
        {
            var r = (PostReaction)i;
            var c = reactionCounters[i - 1];
            if (c > 0)
            {
                commentStr.Append(r.Humanize());
                commentStr.Append(' ');
                commentStr.Append(c.ToMetric());
                commentStr.Append(' ');

                added = true;
            }
        }

        if (added)
        {
            commentStr.Remove(commentStr.Length - 1, 1);
            commentStr.Append(')');
        }
        else
        {
            commentStr.Clear();
        }

        commentStr.Insert(0, $"{totalReactionCount.ToMetric()} ");

        return commentStr.ToString();
    }

    public static void AddToDictOrNull<TValue>(Dictionary<string, object> dictionary, string key, TValue value, bool addNull)
    {
        if (addNull)
        {
            dictionary.Add(key, null);
        }
        else
        {
            dictionary.Add(key, value);
        }
    }
}