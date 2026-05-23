using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Windows.Components;

namespace PvpAutoLb.Windows.Sections;

internal static class MasterButton
{
    private static readonly Vector4 BgEnabled = new(0.18f, 0.55f, 0.30f, 0.85f);
    private static readonly Vector4 BgDisabled = new(0.28f, 0.28f, 0.30f, 0.55f);

    public static void Draw(Configuration cfg)
    {
        var bg = cfg.Enabled ? BgEnabled : BgDisabled;
        var bgHover = bg + new Vector4(0.08f, 0.08f, 0.08f, 0f);
        var bgActive = bg - new Vector4(0.04f, 0.04f, 0.04f, 0f);

        using (ImRaii.PushColor(ImGuiCol.Button, bg))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, bgHover))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, bgActive))
        {
            var label = cfg.Enabled ? "● ENABLED" : "○ DISABLED";
            if (ImGui.Button(label, new Vector2(-1, Layout.MasterButtonHeight * ImGuiHelpers.GlobalScale)))
            {
                cfg.Enabled = !cfg.Enabled;
                cfg.Save();
            }
        }
        Tooltip.OnHover("Master switch. Click to toggle.");
    }
}
