using Friflo.Engine.ECS.Systems;
using Monod.InputModule;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPreUpdateSystem : BaseSystem
{
    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        Rewind.Active = Input.KeyDown(Key.LeftShift);
    }
}
