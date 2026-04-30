# Changelog

All notable changes to PVP Auto LB are documented here.

## v1.0.1.0

Compatibility release for Patch 7.50 / Dalamud API 15. No behavior changes.

### Changed
- **Dalamud API 15.** Bumped `Dalamud.NET.Sdk` to `15.0.0` and `DalamudApiLevel` to 15. ECommons submodule advanced to NightmareXIV/ECommons master at the API15 update commit.

### Fixed
- **Card child-region disposal.** `ImRaii.Child(...)` in API 15 now returns a `ChildDisposable` ref struct instead of `IDisposable`, which broke `Card.Begin`. Replaced the heap-allocated `CompositeDisposable` in `Card.cs` with a `CardScope` ref struct that holds the child handle directly and disposes it before the surrounding style/color pushes (preserving the original LIFO order so ImGui state unwinds correctly).

## v1.0.0.0

First stable release. Builds on v0.1.0.0 with five user-facing features and a full architecture refactor.

### Added
- **Skip doomed targets.** When auto-select is on, the picker estimates each enemy's HP-per-second loss over a 1-second window and skips targets whose predicted time-to-death is shorter than the LB's animation lock (~1.2s). Configurable in **Settings → Filters**.
- **Sound + chat on fire.** Optional. Sound plays one of `/se1`–`/se16`; chat prints `[PvpAutoLb] fired <action> on <target>`. Both off by default. Configurable in **Settings → Feedback**.
- **Player blocklist + duty filter.** Names listed in **Settings → Player blocklist** are never auto-targeted. Allowed-duties checkboxes in **Settings → Filters** scope auto-fire to Crystalline Conflict, Frontline, Rival Wings, Custom Match, or other PvP zones.
- **Session + lifetime stats.** Footer of the main window shows current-session and lifetime totals (fires, attributed kills, total enemies hit). Lifetime persists across reloads. Each line has its own Reset button.
- **Granular readiness pill.** Replaced the single `UNAVAILABLE` state with `OUT OF RANGE` (target out of LB range) and `GAUGE LOW` (LB gauge insufficient).

### Changed
- **About window.** Separate window with author, GitHub repo, issues, discussions, and security advisory links. Open via the info-circle button in the main window's toolbar.
- **UI overhaul.** Compact LB card with icon + name + status pill on the top row and threshold/profile descriptor below. Plugin icons removed from window title bars (the OS-level title already shows the plugin name). Target section is hidden when the plugin is disabled. Master enable/disable lives only on the main window.
- **Save debouncing.** Slider drags no longer hit disk every frame.

### Fixed
- **Hard-target restoration.** When the auto-picker swapped your hard target to fire on a different enemy, your original target was lost. The plugin now snapshots the previous target and restores it ~700 ms after the swap (long enough for the LB animation to land). If you've manually moved off our pick during the window, the restore is skipped — your manual choice wins.

### Architecture
- 35 source files, none over ~120 lines. `MainWindow` and `ConfigWindow` are pure composition (~50 / ~40 lines).
- `Core/` exposes a single `LbCatalog` (replaces the old `IJobLimitBreakModule` registry indirection) plus small focused helpers: `Geo`, `JobLookup`, `HpMath`, `HpTracker`, `SessionStats`, `Feedback`, `DutyDetector`.
- `Windows/Components/` and `Windows/Sections/` separate reusable visual pieces from page composition.
- All magic numbers consolidated in `Core/PvpAutoLbConstants.cs`.
- Hot-path allocations eliminated: per-action throttle keys are cached, `TargetSelector` uses a manual loop with insertion-sort instead of LINQ chains, `FireDecisionMaker` filters with index loops.
- `OnTick` wraps in `try/catch` + logs so a tick-time exception cannot silently unsubscribe the framework callback.

## v0.1.0.0

Renamed from **LastHit** to **PVP Auto LB**. The plugin's `InternalName` changed (`LastHitPlugin` → `PvpAutoLb`), so existing users will need to install fresh — old saved configs do not migrate. Slash commands, window titles, and log prefixes were all updated.

### Added
- **Range-aware targeting.** The picker now reads each LB's `Range`, `EffectRange`, and cast shape from the game's Action sheet:
  - Single-target LBs only consider enemies within the LB's actual cast range, not just the user's scan radius. A 25y LB no longer wastes ticks trying to fire on a 50y target.
  - Circle-around-caster / ground-circle / donut LBs fire when any below-threshold enemy is in the AoE radius.
  - Circle-around-target LBs (e.g. Sky Shatter) pick the candidate whose AoE catches the most below-threshold enemies, breaking ties on lowest effective HP. Cluster matters now.
  - Cone / line / cross LBs fall back to single-target picking — geometric facing isn't modeled, but the LB still fires and catches what the player is facing.
- **Per-job HP thresholds.** Each PvP job can now have its own threshold mode + value, overriding the global setting. Configure under the **Per-job override** card in settings; the active job's override is keyed off whatever you're currently logged into. Falls back to the global threshold when no override exists.
- **Shield-aware HP.** Threshold checks now compare against effective HP (`CurrentHp + ShieldHp`), so the LB doesn't fire prematurely on a target whose shield will eat the damage. The hero card's HP bar shows the shield as a gold strip stacked past the green segment.
- **Status window LB card** now shows the LB's range and shape (e.g. `25y · single-target` or `PBAoE 5y · catches 3`) under the threshold line. When firing on an AoE LB, it reports the count of enemies caught.

### Changed
- `Configuration.Version` bumped from 1 to 2 to track the per-job-thresholds field. Existing v1 configs deserialize with `PerJobThresholds` defaulting to empty; no manual migration required.
- The hero card now reflects the controller's actual fire pick (relevant for AoE LBs that prefer cluster size over lowest HP).

## v0.0.0.5

### Added
- **Defensive/support LB detection.** Jobs whose PvP Limit Break is defensive or support-focused (Paladin, Warrior, Bard, Black Mage, Astrologian, Dancer, Reaper, Sage, Pictomancer) are now recognized as out of scope. The Limit Break card surfaces an amber `DEFENSIVE` pill with a "Support LB — not auto-fired" note instead of an HP-threshold label, so players don't expect the plugin to fire on those jobs.
- README job tables now include a 🛡 status for support LBs alongside ✅ / ❔.

## v0.0.0.4

### Fixed
- **PvP target detection.** Enemy players in Crystalline Conflict, Frontlines, and Rival Wings were not showing up in the Target section. The hostile filter relied on `StatusFlags.Hostile`, which is driven by PvE aggro state and is not reliably set on opposing-team players in PvP. Replaced with ECommons' nameplate-color `IsHostile()` check, which correctly identifies PvP enemy players as well as hostile BattleNpcs (mechs, NPCs, summons in Frontlines / Rival Wings).

## v0.0.0.3
- Release packaging fixes; download-count workflow; job compatibility notes in README.

## v0.0.0.2
- UI polish: empty-card text clipping at small window sizes.

## v0.0.0.1
- Initial release.
