namespace AzerothMemories.WebServer.Database.Records;

public enum BlizzardUpdateType
{
    Default = 0,

    Account = 0,
    Account_China = BlizzardRegion.China,
    Account_Europe = BlizzardRegion.Europe,
    Account_Korea = BlizzardRegion.Korea,
    Account_Taiwan = BlizzardRegion.Taiwan,
    Account_UnitedStates = BlizzardRegion.UnitedStates,
    Account_Patreon,

    Account_Count,

    Character = 0,
    Character_Renders,
    Character_Achievements,
    Character_Count,

    Guild = 0,
    Guild_Roster,
    Guild_Achievements,
    Guild_Count,
}