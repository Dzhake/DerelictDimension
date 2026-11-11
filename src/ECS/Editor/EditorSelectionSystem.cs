using Monod.ECS.Systems;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionSystem : SystemBase
{
    public int SelectedId;
    public HashSet<long> LastSelectedPids;

    public override void Update(SystemCtx ctx)
    {
        Log.Information("EDITOR");
    }
}
