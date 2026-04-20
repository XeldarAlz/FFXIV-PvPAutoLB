using Dalamud.Configuration;
using System;

namespace LastHitPlugin;

public enum ThresholdMode
{
    Percent,
    Absolute,
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool Enabled { get; set; } = true;

    public ThresholdMode ThresholdMode { get; set; } = ThresholdMode.Percent;
    public float HpThresholdPercent { get; set; } = 30f;
    public uint HpThresholdAbsolute { get; set; } = 7000;

    public bool AutoSelectLowestHp { get; set; } = true;
    public float AutoSelectRangeYalms { get; set; } = 30f;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
