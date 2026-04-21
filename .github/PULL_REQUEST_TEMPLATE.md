## What

<!-- One or two sentences on what this PR changes. -->

## Why

<!-- The motivating problem, linked issue, or user-visible behavior this fixes. -->

Closes #

## How to test

<!--
Minimum steps a reviewer can run to verify the change. LastHit is PvP-only, so testing usually means queueing Crystalline Conflict on the affected job. For UI-only changes, describe what to click.
-->

## Checklist

- [ ] `dotnet build -c Release` passes
- [ ] Verified in-game on the affected job(s)
- [ ] If this changes user-visible behavior, README is updated
- [ ] If this touches LB dispatch, relevant `[LastHit]` log lines make the sequence auditable
