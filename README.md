<p align="center">
  <img src="LastHitPlugin/images/icon.png" width="220" alt="LastHit icon" />
</p>

<h1 align="center">LastHit</h1>

---

## What it does

Monitors enemy HP during PvP. When the target's HP drops below the configured threshold, uses your job's PvP Limit Break. Good for classes like Ninja or Machinist to auto kill enemies.

Jobs whose PvP Limit Break is defensive or support-focused (e.g., Paladin's Phalanx) are flagged in the status window and not auto-fired — the low-HP-enemy gate doesn't apply to them.

## Features

- Configurable threshold, expressed as either a percent of max HP or an absolute HP value.
- Optional auto-target: picks the lowest-HP hostile in range when no manual target is set.
- Works for every PvP job.
- Status window: current target, HP / max HP / percent, threshold state, time since last fire.
- Respects the game's action availability and animation lock.

## Install

LastHit is distributed through a custom Dalamud plugin repository.

1. In-game, run `/xlsettings` → **Experimental**.
2. Under **Custom Plugin Repositories**, paste:
   ```
   https://raw.githubusercontent.com/XeldarAlz/FFXIV-LastHit/master/repo.json
   ```
   Tick **Enabled**, click the **+**, then **Save and Close**.
3. Open `/xlplugins` → **All Plugins**, search for **LastHit**, and install.

Updates are delivered automatically whenever a new release is cut.

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

## Job compatibility

PvP Limit Breaks are resolved dynamically from game data, so every job is wired up automatically. Jobs whose LB is defensive or support-focused are flagged in the status window and not auto-fired — the low-HP-enemy gate doesn't apply to them. The table below tracks what has actually been verified in live PvP matches. If you test a job, please open an issue or PR so this list can be updated.

**Legend:** ✅ confirmed working · ❔ not tested yet · 🛡 defensive/support — not auto-fired (out of scope)

### Tanks
| Job | Status | Notes |
|---|---|---|
| Paladin | 🛡 | Phalanx — defensive (party barrier + Stoneskin) |
| Warrior | 🛡 | Primal Scream — defensive |
| Dark Knight | ✅ | Confirmed working in CC |
| Gunbreaker | ✅ | Confirmed working in CC |

### Healers
| Job | Status | Notes |
|---|---|---|
| White Mage | ✅ | Confirmed working in CC |
| Scholar | ❔ | Not tested yet |
| Astrologian | 🛡 | Celestial River — support |
| Sage | 🛡 | Mesotes — support |

### Melee DPS
| Job | Status | Notes |
|---|---|---|
| Monk | ✅ | Confirmed working in CC |
| Dragoon | ✅ | Confirmed working in CC |
| Ninja | ✅ | Confirmed working in CC |
| Samurai | ✅ | Confirmed working in CC |
| Reaper | 🛡 | Tenebrae Lemurum — support |
| Viper | ✅ | Confirmed working in CC |

### Physical Ranged DPS
| Job | Status | Notes |
|---|---|---|
| Bard | 🛡 | Final Fantasia — support |
| Machinist | ✅ | Confirmed working in CC |
| Dancer | 🛡 | Contradance — support |

### Magical Ranged DPS
| Job | Status | Notes |
|---|---|---|
| Black Mage | 🛡 | Soul Resonance — support |
| Summoner | ❔ | Not tested yet |
| Red Mage | ❔ | Not tested yet |
| Pictomancer | 🛡 | Advent of Chocobastion — defensive |

## License

AGPL-3.0-or-later. See [LICENSE.md](LICENSE.md).
