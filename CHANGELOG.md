# Changelog

All notable changes to LastHit are documented here.

## v0.0.0.4

### Fixed
- **PvP target detection.** Enemy players in Crystalline Conflict, Frontlines, and Rival Wings were not showing up in the Target section. The hostile filter relied on `StatusFlags.Hostile`, which is driven by PvE aggro state and is not reliably set on opposing-team players in PvP. Replaced with ECommons' nameplate-color `IsHostile()` check, which correctly identifies PvP enemy players as well as hostile BattleNpcs (mechs, NPCs, summons in Frontlines / Rival Wings).

## v0.0.0.3
- Release packaging fixes; download-count workflow; job compatibility notes in README.

## v0.0.0.2
- UI polish: empty-card text clipping at small window sizes.

## v0.0.0.1
- Initial release.
