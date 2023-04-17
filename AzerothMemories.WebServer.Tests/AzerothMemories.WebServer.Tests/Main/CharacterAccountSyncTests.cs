using AzerothMemories.WebServer.Tests.Common;
using FluentAssertions;
using Xunit;

namespace AzerothMemories.WebServer.Tests.Main;

public class CharacterAccountSyncTests : BaseTestHelper
{
    [Fact]
    public async Task DefaultIsFalse()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");
        var count = 20;

        await CreateRandomCharacters(account1, count);

        var characters = await CommonServices.CharacterServices.TryGetAllAccountCharacters(account1.Id);
        characters.Count.Should().Be(count);

        foreach (var character in characters)
        {
            character.Key.Should().BeGreaterThan(0);
            character.Value.Id.Should().BeGreaterThan(0);
            character.Value.Name.Should().NotBeNull();
            character.Value.AccountSync.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CanChangeOwnCharacterAccountsSync()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        await CreateRandomCharacters(account1, 3);

        var characters = await CommonServices.CharacterServices.TryGetAllAccountCharacters(account1.Id);
        foreach (var character in characters)
        {
            var result = await CommonServices.Commander.Call(new Character_TryChangeCharacterAccountSync(session1, character.Key, true));
            result.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanNotChangeOtherAccountsSync1()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        await CreateRandomCharacters(account1, 3);

        var characters = await CommonServices.CharacterServices.TryGetAllAccountCharacters(account1.Id);
        foreach (var character in characters)
        {
            var session2 = SessionFactory.CreateSession();
            var result = await CommonServices.Commander.Call(new Character_TryChangeCharacterAccountSync(session2, character.Key, true));
            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CanNotChangeOtherAccountsSync2()
    {
        var session1 = SessionFactory.CreateSession();
        var account1 = await CreateUser(session1, "Bob");

        await CreateRandomCharacters(account1, 3);

        var characters = await CommonServices.CharacterServices.TryGetAllAccountCharacters(account1.Id);
        foreach (var character in characters)
        {
            var session2 = SessionFactory.CreateSession();
            var account2 = await CreateUser(session2, "Bob");

            var result = await CommonServices.Commander.Call(new Character_TryChangeCharacterAccountSync(session2, character.Key, true));
            result.Should().BeFalse();
        }
    }
}