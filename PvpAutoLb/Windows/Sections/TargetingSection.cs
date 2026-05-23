using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class TargetingSection
{
    public static void Draw(Configuration cfg)
    {
        Styling.SectionLabel("Targeting");

        using (Card.Begin("##targeting", Layout.TargetingCardHeight * ImGuiHelpers.GlobalScale, Styling.CardBg, Styling.CardBorderDim))
        {
            var auto = cfg.AutoSelectLowestHp;
            if (ImGui.Checkbox("Auto-select lowest-HP hostile in range", ref auto))
            {
                cfg.AutoSelectLowestHp = auto;
                cfg.Save();
            }
            Tooltip.OnHover("When on, the plugin continuously scans all visible hostiles and targets the one with the lowest HP — overriding your manual hard target.");

            using (ImRaii.Disabled(!cfg.AutoSelectLowestHp))
            {
                var range = cfg.AutoSelectRangeYalms;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.SliderFloat("##range", ref range, 5f, 50f, "Range: %.0f y"))
                {
                    cfg.AutoSelectRangeYalms = range;
                    cfg.SaveDebounced();
                }

                ImGui.Spacing();
                using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                    ImGui.TextUnformatted(RangeHint(range));
            }
        }
    }

    private static string RangeHint(float range) => range switch
    {
        <= 8f => "≈ melee range",
        <= 20f => "≈ mid range",
        <= 35f => "≈ ranged combat",
        _ => "≈ whole arena",
    };
}
