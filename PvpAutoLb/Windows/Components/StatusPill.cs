using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using PvpAutoLb.Core;

namespace PvpAutoLb.Windows.Components;

internal static class StatusPill
{
    public static (Vector4 Color, string Text) Resolve(bool firing, bool wouldFire, LbReadyReason readiness, bool enabled)
    {
        if (firing)    return (Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast), "FIRING");
        if (wouldFire) return (new Vector4(0.75f, 0.45f, 0.48f, 1f), "PAUSED");
        if (!enabled)  return (Styling.TextMuted, "DISABLED");
        return readiness switch
        {
            LbReadyReason.Ready      => (Styling.AccentOrange, "READY"),
            LbReadyReason.OutOfRange => (Styling.AccentAmber, "OUT OF RANGE"),
            _                        => (Styling.TextMuted, "GAUGE LOW"),
        };
    }

    public static void Draw(bool firing, bool wouldFire, LbReadyReason readiness, bool enabled)
    {
        var (color, text) = Resolve(firing, wouldFire, readiness, enabled);
        using (ImRaii.PushColor(ImGuiCol.Text, color))
            ImGui.TextUnformatted(text);
    }

    public static void DrawSupport()
    {
        using (ImRaii.PushColor(ImGuiCol.Text, Styling.AccentAmber))
            ImGui.TextUnformatted("DEFENSIVE");
    }

    public static float MeasureWidth(string text) => ImGui.CalcTextSize(text).X;
}
