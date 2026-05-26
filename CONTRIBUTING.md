# Contributing

Thanks for taking an interest. This is a small solo project, but PRs are welcome and I'll review them.

## Quick start

```bash
git clone --recurse-submodules https://github.com/XeldarAlz/FFXIV-AutoPVPLimitBreak.git
cd FFXIV-AutoPVPLimitBreak
dotnet build PvpAutoLb.sln -c Release
```

You need the .NET 9 SDK. The plugin requires Dalamud at runtime; CI pulls a Dalamud dev build automatically and that's enough to compile. See `.github/workflows/pr-build.yml` if you want to reproduce CI locally.

Load the built plugin via `/xlsettings` → **Experimental** → **Dev Plugin Locations**, pointing at `PvpAutoLb/bin/x64/Release/PvpAutoLb/PvpAutoLb.dll`.

## Project layout

- `PvpAutoLb/Core/` — HP monitoring, target selection, LB dispatch.
- `PvpAutoLb/Windows/` — ImGui status and settings windows.
- `PvpAutoLb/` — plugin entry points, config, command wiring.
- `ECommons/` — submodule, shared Dalamud helpers. Don't patch this directly; upstream it.

Keep logic small and direct. This plugin has one job.

## Before you open a PR

1. `dotnet build -c Release` cleanly.
2. Test in-game on at least one PvP job, in an actual Crystalline Conflict match. Auto PVP LB is PvP-only — nothing else counts as "tested."
3. Keep the diff focused. One concern per PR.
4. Match the existing style. No heavy abstractions "for later."
5. If your change affects what a user sees or types (commands, window layout, settings), update the README.

## Good first issues

Check the tracker for anything labeled `good first issue`. The per-job LB verification matrix is the lowest-friction way to help — no code, just play the game.

## Adding or fixing a job's LB

1. Confirm the action IDs (base LB and any follow-ups).
2. Add/adjust the mapping.
3. Test in a real CC match with that job. Note any targeting specifics (ground-targeted, self-buff, chained) in the PR.
4. Make sure the `[PvpAutoLb]` log lines make the sequence auditable.

## Security

Please don't file public issues for security problems — see [SECURITY.md](SECURITY.md).

## Code of conduct

See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). Be decent.

## License

By contributing, you agree your contributions are licensed under AGPL-3.0-or-later, the same as the project.
