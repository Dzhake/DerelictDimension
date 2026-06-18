using DerelictDimension.ECS.Physics;
using Monod.InputModule;
using Monod.MathModule;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Ai;

public struct PlayerAi : IComponent, IAi
{
    public float TargetXVel = 200;
    public float Acceleration = 2000;
    public float AirAcceleration = 600;
    public float JumpStrength = 500;

    public PlayerAi()
    {
    }

    public readonly void PostUpdate(Entity entity) { }

    public readonly void PreUpdate(Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<MobileComponent>()) return;

        ref var mobile = ref data.Get<MobileComponent>();

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref mobile.Velocity.X;
        float xAccel = mobile.InAir ? AirAcceleration : Acceleration;
        float targetX = xvel;
        if (left && right)
            targetX = 0;
        else if (left)
            targetX = -TargetXVel;
        else if (right)
            targetX = TargetXVel;

        float frameAccel = xAccel * Time.DeltaTime;
        MathM.LerpFloat(ref xvel, targetX, frameAccel);

        if (Input.KeyDown(Key.Space) && !mobile.InAir)
        {
            mobile.Velocity.Y -= JumpStrength;
            mobile.SupportingEntityId = -1;
        }
    }
}
