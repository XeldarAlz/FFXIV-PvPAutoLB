<p align="center">
  <img src="LastHitPlugin/images/icon.png" width="220" alt="LastHit icon" />
</p>

<h1 align="center">LastHit (WORK IN PROGRESS)</h1>

<p align="center">
  Fires your PvP Limit Break at a configurable HP threshold.
</p>

---

## What it does

Monitors enemy HP during PvP. When the target's HP drops below the configured threshold, uses your job's PvP Limit Break. Good for classes like Ninja or Machinist.

## Features

- Configurable threshold, expressed as either a percent of max HP or an absolute HP value.
- Optional auto-target: picks the lowest-HP hostile in range when no manual target is set.
- Works for every PvP job.
- Status window: current target, HP / max HP / percent, threshold state, time since last fire.
- Respects the game's action availability and animation lock.

## Install

1. `/xlsettings` → **Experimental** → add the full path to `LastHitPlugin.dll` under **Dev Plugin Locations**.
2. `/xlplugins` → **Dev Tools → Installed Dev Plugins** → enable **LastHit**.

Build output is at `LastHitPlugin/bin/x64/Release/LastHitPlugin/`.

## Commands

| Command | Action |
|---|---|
| `/lasthit` | Toggle the status window |
| `/lasthit config` | Open settings |

## Configuration

- **Enabled** — master switch.
- **Threshold mode** — percent of max HP, or absolute HP value.
- **Threshold value** — slider (percent) or numeric input (absolute).
- **Auto-select lowest-HP enemy** — used when no manual target is set.
- **Auto-select range** — yalms, 5–50.

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md).
