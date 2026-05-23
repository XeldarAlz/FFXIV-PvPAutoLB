using ECommons.DalamudServices;
using LuminaTerritory = Lumina.Excel.Sheets.TerritoryType;

namespace PvpAutoLb.Core;

internal static class DutyDetector
{
    // FFXIV TerritoryIntendedUse row ids. These have been stable across recent
    // patches; if SE ever renumbers them the only consequence is duties falling
    // through to DutyMask.Other.
    private const byte IntendedUseFrontline = 31;
    private const byte IntendedUseCrystallineConflict = 32;
    private const byte IntendedUseRivalWings = 36;
    private const byte IntendedUseCustomMatch = 41;

    public static DutyMask Current()
    {
        if (!Svc.ClientState.IsPvP) return DutyMask.None;

        var sheet = Svc.Data.GetExcelSheet<LuminaTerritory>();
        var row = sheet?.GetRowOrDefault(Svc.ClientState.TerritoryType);
        if (row == null) return DutyMask.Other;

        return row.Value.TerritoryIntendedUse.RowId switch
        {
            IntendedUseFrontline           => DutyMask.Frontline,
            IntendedUseCrystallineConflict => DutyMask.CrystallineConflict,
            IntendedUseRivalWings          => DutyMask.RivalWings,
            IntendedUseCustomMatch         => DutyMask.CustomMatch,
            _                              => DutyMask.Other,
        };
    }
}
