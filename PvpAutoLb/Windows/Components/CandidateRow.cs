using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Components;

internal static class CandidateRow
{
    private static readonly Vector4 BgBelow = new(0.18f, 0.07f, 0.08f, 0.45f);
    private static readonly Vector4 BarBg = new(0.05f, 0.06f, 0.07f, 0.80f);

    public static void Draw(IBattleChara target, Configuration cfg, uint jobId)
    {
        var cur = target.CurrentHp;
        var max = target.MaxHp;
        var fraction = max == 0 ? 0f : (float)cur / max;
        var pct = fraction * 100f;
        var below = HpMath.IsBelowThreshold(target, cfg, jobId);
        var distance = Geo.DistanceToPlayer(target);
        var rowBg = below ? BgBelow : Styling.CardBgSoft;

        using (ImRaii.PushColor(ImGuiCol.ChildBg, rowBg))
        using (ImRaii.PushColor(ImGuiCol.Border, Styling.CardBorderDim))
        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(8, 4) * ImGuiHelpers.GlobalScale))
        using (ImRaii.Child("##cand_" + target.GameObjectId, new Vector2(-1, Layout.CandidateRowHeight * ImGuiHelpers.GlobalScale), true,
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var avail = ImGui.GetContentRegionAvail().X;
            var nameW = avail * 0.38f;
            var distW = avail * 0.12f;
            var barW = avail * 0.50f;

            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextSecondary))
                ImGui.TextUnformatted(Truncate(target.Name.TextValue, nameW));
            ImGui.SameLine(nameW);
            using (ImRaii.PushColor(ImGuiCol.Text, Styling.TextDim))
                ImGui.TextUnformatted($"{distance:F0}y");
            ImGui.SameLine(nameW + distW);

            using (ImRaii.PushColor(ImGuiCol.PlotHistogram, below ? Styling.AccentRed : Styling.AccentGreen))
            using (ImRaii.PushColor(ImGuiCol.FrameBg, BarBg))
                ImGui.ProgressBar(fraction, new Vector2(barW, Layout.HpBarHeightCandidate * ImGuiHelpers.GlobalScale), $"{pct:F0}%%");
        }
    }

    private static string Truncate(string name, float maxWidth)
    {
        if (ImGui.CalcTextSize(name).X <= maxWidth) return name;
        while (name.Length > 3 && ImGui.CalcTextSize(name + "…").X > maxWidth)
            name = name[..^1];
        return name + "…";
    }
}
