namespace AzerothMemories.WebServer.Database.Records;

public enum BlizzardUpdateType
{
    Default = 0,

    Account = 0,
    Account_China,
    Account_Europe,
    Account_Korea,
    Account_Taiwan,
    Account_UnitedStates,

    Account_Patreon,

    Account_China_ClassicEra,
    Account_Europe_ClassicEra,
    Account_Korea_ClassicEra,
    Account_Taiwan_ClassicEra,
    Account_UnitedStates_ClassicEra,

    Account_China_ClassicProgression,
    Account_Europe_ClassicProgression,
    Account_Korea_ClassicProgression,
    Account_Taiwan_ClassicProgression,
    Account_UnitedStates_ClassicProgression,

    Account_China_ClassicAnniversary,
    Account_Europe_ClassicAnniversary,
    Account_Korea_ClassicAnniversary,
    Account_Taiwan_ClassicAnniversary,
    Account_UnitedStates_ClassicAnniversary,

    Account_Count,

    Character = 0,
    Character_Renders,
    Character_Achievements,
    Character_Mounts,
    Character_AchievementStatistics,
    Character_Count,

    Guild = 0,
    Guild_Roster,
    Guild_Achievements,
    Guild_Count
}