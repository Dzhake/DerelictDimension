using DerelictDimension.ECS.Physics.Components;
using Monod.ECS.DefaultComponents;
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

    public readonly void PostUpdate(Entity entity, EntityStore store, CommandBuffer _) { }

    public readonly void PreUpdate(Entity entity, EntityStore store, CommandBuffer _)
    {
        var data = entity.Data;
        if (!data.Has<MobileComponent>() || !data.Has<Transform2D>()) return;

        ref var mobile = ref data.Get<MobileComponent>();
        ref var transform = ref data.Get<Transform2D>();

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref mobile.Velocity.X;
        float xAccel = mobile.InAir ? AirAcceleration : Acceleration;

        if (mobile.SupportingEntityId != -1)
        {
            var supportingEnt = store.GetEntityById(mobile.SupportingEntityId);
            var supportingData = supportingEnt.Data;
            if (!supportingEnt.IsNull && supportingData.Has<SupportComponent>())
            {
                ref var support = ref supportingData.Get<SupportComponent>();
                xAccel *= support.AccelerationMult;
            }
        }

        float targetX = xvel;
        if (left && right)
        {
            targetX = 0;
        }
        else if (left)
        {
            targetX = -TargetXVel;
            transform.FlipX = true;
        }
        else if (right)
        {
            targetX = TargetXVel;
            transform.FlipX = false;
        }

        float frameAccel = xAccel * Time.DeltaTime;
        MathM.LerpFloat(ref xvel, targetX, frameAccel);

        if (Input.KeyDown(Key.Space) && !mobile.InAir)
        {
            mobile.Velocity.Y -= JumpStrength;
            mobile.SupportingEntityId = -1;
        }
    }
}
