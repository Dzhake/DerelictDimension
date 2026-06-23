using Friflo.Engine.ECS.Systems;
using Monod.InputModule;

namespace DerelictDimension.ECS.Rewinding;

public class RewindPreUpdateSystem : BaseSystem
{
    protected override void OnUpdateGroup()
    {
        base.OnUpdateGroup();
        bool enableRewind = Rewind.Active;
        if (Input.KeyPressed(Key.LeftShift))
            enableRewind = !enableRewind;
        ref int rewindSpeed = ref Rewind.RewindSpeed;

        // set default value when rewind is not active
        if (!Rewind.Active)
        {
            rewindSpeed = -1;
        }


        // when rewind starts, very last frame becomes invalid, so we need to exclude it.
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
