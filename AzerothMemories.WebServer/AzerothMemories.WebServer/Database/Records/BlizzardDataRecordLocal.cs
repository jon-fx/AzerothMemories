namespace AzerothMemories.WebServer.Database.Records;

public class BlizzardDataRecordLocal
{
    public string En_Us;
    public string Ko_Kr;
    public string Fr_Fr;
    public string De_De;
    public string Zh_Cn;
    public string Es_Es;
    public string Zh_Tw;
    public string En_Gb;
    public string Es_Mx;
    public string Ru_Ru;
    public string Pt_Br;
    public string It_It;
    public string Pt_Pt;

    public bool Update(BlizzardDataRecordLocal data)
    {
        var changed = false;

        CheckAndChange.Check(ref En_Us, data.En_Us, ref changed);
        CheckAndChange.Check(ref Ko_Kr, data.Ko_Kr, ref changed);
        CheckAndChange.Check(ref Fr_Fr, data.Fr_Fr, ref changed);
        CheckAndChange.Check(ref De_De, data.De_De, ref changed);
        CheckAndChange.Check(ref Zh_Cn, data.Zh_Cn, ref changed);
        CheckAndChange.Check(ref Es_Es, data.Es_Es, ref changed);
        CheckAndChange.Check(ref Zh_Tw, data.Zh_Tw, ref changed);
        CheckAndChange.Check(ref En_Gb, data.En_Gb, ref changed);
        CheckAndChange.Check(ref Es_Mx, data.Es_Mx, ref changed);
        CheckAndChange.Check(ref Ru_Ru, data.Ru_Ru, ref changed);
        CheckAndChange.Check(ref Pt_Br, data.Pt_Br, ref changed);
        CheckAndChange.Check(ref It_It, data.It_It, ref changed);
        CheckAndChange.Check(ref Pt_Pt, data.Pt_Pt, ref changed);

        return changed;
    }

    //public bool IsNull()
    //{
    //    return string.IsNullOrEmpty(En_Us) &&
    //           string.IsNullOrEmpty(Ko_Kr) &&
    //           string.IsNullOrEmpty(Fr_Fr) &&
    //           string.IsNullOrEmpty(De_De) &&
    //           string.IsNullOrEmpty(Ko_KR) &&
    //           string.IsNullOrEmpty(En_US) &&
    //           string.IsNullOrEmpty(Es_MX) &&
    //           string.IsNullOrEmpty(Pt_BR) &&
    //           string.IsNullOrEmpty(Es_ES) &&
    //           string.IsNullOrEmpty(Zh_CN) &&
    //           string.IsNullOrEmpty(Fr_FR) &&
    //           string.IsNullOrEmpty(De_DE);
    //}
}