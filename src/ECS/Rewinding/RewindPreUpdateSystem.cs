using Friflo.Engine.ECS.Systems;
using Monod.InputModule;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPreUpdateSystem : BaseSystem
{
    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        bool enableRewind = Input.KeyDown(Key.LeftShift);
        ref int rewindSpeed = ref Rewind.RewindSpeed;
        if (!Rewind.Active)
        {
            rewindSpeed = -1;
        }
        if (enableRewind && !Rewind.Active)
        {
            RewindPostUpdateSystem.LastValidFrame = Rewind.CurrentFrame - 1;
        }
        if (enableRewind && Input.KeyPressed(Key.Up))
        {
            if (rewindSpeed == -1 || rewindSpeed == 0) rewindSpeed++;
            else if (rewindSpeed < 0) rewindSpeed /= 2;
            else rewindSpeed *= 2;
        }
        if (enableRewind && Input.KeyPressed(Key.Down))
        {
            if (rewindSpeed == 1 || rewindSpeed == 0) rewindSpeed--;
            else if (rewindSpeed < 0) rewindSpeed *= 2;
            else rewindSpeed /= 2;
        }
        Rewind.Active = enableRewind;
    }
}
