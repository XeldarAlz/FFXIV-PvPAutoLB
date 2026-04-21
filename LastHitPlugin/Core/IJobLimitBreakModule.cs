using System.Collections.Generic;

namespace LastHitPlugin.Core;

internal interface IJobLimitBreakModule
{
    IReadOnlyList<uint> Resolve(uint classJobId);
}
