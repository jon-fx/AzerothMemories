using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using System.Text;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public sealed class PostCreateTests : BaseTestHelper
{
    [Fact]
    public async Task CantCreatePostWhenNotLoggedIn1()
    {
        var session = SessionFactory.CreateSession();
        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = ZExtensions.MinPostTime.ToUnixTimeMilliseconds(),
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.SessionNotFound);
    }

    [Fact]
    public async Task CantCreatePostWhenNotLoggedIn2()
    {
        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            TimeStamp = ZExtensions.MinPostTime.ToUnixTimeMilliseconds(),
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.SessionNotFound);
    }

    [Fact]
    public async Task CantCreatePostWithLongCommentText()
    {
        var session = SessionFactory.CreateSession();
        var commentTextBuilder = new StringBuilder();
        for (var i = 0; i < ZExtensions.MaxCommentLength; i++)
        {
            commentTextBuilder.Append(i);
        }

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            Comment = commentTextBuilder.ToString(),
            TimeStamp = ZExtensions.MinPostTime.ToUnixTimeMilliseconds(),
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.CommentTooLong);
    }

    [Fact]
    public async Task CantCreatePostWithLowTimeStamp()
    {
        var session = SessionFactory.CreateSession();
        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime - Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.InvalidTime);
    }

    [Fact]
    public async Task CantCreatePostWithHighTimeStamp()
    {
        var session = SessionFactory.CreateSession();
        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (SystemClock.Instance.GetCurrentInstant() + Duration.FromSeconds(5)).ToUnixTimeMilliseconds(),
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.InvalidTime);
    }

    [Fact]
    public async Task CanNotCreatePostWithoutAccountTag()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.InvalidTags);
    }

    [Fact]
    public async Task CanNotCreatePostWithoutRegionTag()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.InvalidTags);
    }

    [Fact]
    public async Task CanNotCreatePostWithoutTypeTag()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.InvalidTags);
    }

    [Fact]
    public async Task CanCreatePostWithoutMainTag()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
            },

            ImageData = new List<byte[]>
            {
                GetImageData(128, 128),
                GetImageData(128, 128),
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.Success);
    }

    [Fact]
    public async Task CanNotCreatePostWithoutImageData()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>()
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.NoImageMustContainText);
    }

    [Fact]
    public async Task CanCreatePost()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>
            {
                GetImageData(128, 128),
                GetImageData(128, 128),
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.Success);
    }

    [Fact]
    public async Task CanNotCreatePostWithNullData()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>
            {
                null,
                null,
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.UploadFailed);
    }

    [Fact]
    public async Task CanNotCreatePostWithZeroData_BAD_TEST_SINCE_WE_CAN_CREATE_POSTS_WITHOUT_IMAGES()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>
            {
                GetImageData(128, 128),
                GetImageData(128, 128),
                GetImageData(128, 128),
                GetImageData(128, 128),
                GetImageData(128, 128),
                GetImageData(128, 128),
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.Success);
    }

    [Fact]
    public async Task CanNotCreatePostWithTooManyImages()
    {
        var session = SessionFactory.CreateSession();
        var account = await CreateUser(session, "Bob");

        var result = await CommonServices.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>
            {
                new byte[128],
                new byte[128],
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.UploadFailed);
    }

    public static async Task<AddMemoryResult> CreateValidPost(CommonServices services, Session session, AccountViewModel account)
    {
        var result = await services.Commander.Call(new Post_TryPostMemory
        {
            Session = session,
            TimeStamp = (ZExtensions.MinPostTime + Duration.FromMilliseconds(1)).ToUnixTimeMilliseconds(),

            SystemTags = new HashSet<string>
            {
                PostTagInfo.GetTagString(PostTagType.Account, account.Id),
                PostTagInfo.GetTagString(PostTagType.Region, BlizzardRegion.None.ToValue()),
                PostTagInfo.GetTagString(PostTagType.Type, 1),
                PostTagInfo.GetTagString(PostTagType.Main, 1)
            },

            ImageData = new List<byte[]>
            {
                GetImageData(128, 128),
            }
        });

        result.Should().NotBeNull();
        result.Result.Should().Be(AddMemoryResultCode.Success);
        result.AccountId.Should().Be(account.Id);
        result.PostId.Should().BeGreaterThan(0);

        return result;
    }
}