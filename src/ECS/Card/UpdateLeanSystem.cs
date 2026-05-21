using Friflo.Engine.ECS.Systems;
using Monod.InputModule;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Card;

public class UpdateLeanSystem : BaseSystem
{
    public static InputActionIndex LeanLeft;
    public static InputActionIndex LeanRight;
    public static readonly float LeanSpeed = 8f;
    public static float Lean;
    public static float Target;

    protected override void OnUpdateGroup()
    {
        float target = 0;

        bool leanLeft = Input.ActionDown(LeanLeft);
        bool leanRight = Input.ActionDown(LeanRight);
        if (leanLeft && !leanRight)
        {
            target = -1;
        }
        if (leanRight && !leanLeft)
        {
            target = 1;
        }

        if (Lean < target)
        {
            Lean += Time.DeltaTime * LeanSpeed;
            if (Lean >= target) Lean = target;
        }
        else if (Lean > target)
        {
            Lean -= Time.DeltaTime * LeanSpeed;
            if (Lean <= target) Lean = target;
        }
        Target = target;
    }

    public static float GetActualLean()
    {
        return (float)Math.Pow(Lean, 3);
    }
}
