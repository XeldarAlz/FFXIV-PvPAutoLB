using System.Numerics;

namespace PvpAutoLb.Windows.Components;

internal static class CardBorders
{
    public static Vector4 Resolve(bool ready, bool enabled, bool wouldFire, bool firing)
    {
        if (firing) return Styling.PulseColor(Styling.AccentRed, Styling.AccentRedBright, Styling.PulseFast);
        if (wouldFire) return Styling.BorderWouldFire;
        if (ready && enabled) return Styling.AccentOrange;
        return Styling.CardBorderDim;
    }
}
