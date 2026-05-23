using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class FilterSection
{
    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Filters");

        using (Card.Begin("##filters", Layout.FilterCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            DrawSkipDoomed(cfg);
            ImGui.Spacing();
            DrawDutyMask(cfg);
        }
    }

    private static void DrawSkipDoomed(Configuration cfg)
    {
        var skip = cfg.SkipDoomedTargets;
        if (ImGui.Checkbox("Skip targets that will die before the LB lands", ref skip))
        {
            cfg.SkipDoomedTargets = skip;
            cfg.Save();
        }
        Tooltip.OnHover("Estimates each enemy's HP-per-second loss; skips them if predicted death is sooner than ~1.2s (typical LB animation lock).");
    }

    private static void DrawDutyMask(Configuration cfg)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
            ImGui.TextUnformatted("ALLOWED DUTIES");

        var mask = cfg.EnabledDuties;
        var changed = false;

        changed |= DrawDutyToggle(ref mask, DutyMask.CrystallineConflict, "Crystalline Conflict");
        ImGui.SameLine();
        changed |= DrawDutyToggle(ref mask, DutyMask.Frontline, "Frontline");

        changed |= DrawDutyToggle(ref mask, DutyMask.RivalWings, "Rival Wings");
        ImGui.SameLine();
        changed |= DrawDutyToggle(ref mask, DutyMask.CustomMatch, "Custom Match");

        changed |= DrawDutyToggle(ref mask, DutyMask.Other, "Other PvP");

        if (changed)
        {
            cfg.EnabledDuties = mask;
            cfg.Save();
        }
    }

    private static bool DrawDutyToggle(ref DutyMask mask, DutyMask flag, string label)
    {
        var on = (mask & flag) != 0;
        if (!ImGui.Checkbox(label, ref on)) return false;
        mask = on ? mask | flag : mask & ~flag;
        return true;
    }
}
