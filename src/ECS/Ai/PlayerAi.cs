using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Ai;

public record struct PlayerAi : IComponent, IAi
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

        Rewind.Keep(entity, ref transform);
        Rewind.Keep(entity, ref mobile);

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref mobile.Velocity.X;
        float xAccel = mobile.InAir ? AirAcceleration : Acceleration;
        xAccel *= Time.DeltaTime;

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

        if (left && !right)
        {
            float targetX = -TargetXVel;
            transform.FlipX = true;
            if (targetX < xvel)
            {
                xvel -= xAccel;
                if (xvel < targetX) xvel = targetX;
            }
        }
        else if (right && !left)
        {
            float targetX = TargetXVel;
            transform.FlipX = false;
            if (targetX > xvel)
            {
                xvel += xAccel;
                if (xvel > targetX) xvel = targetX;
            }
        }

        if (Input.KeyDown(Key.Space) && !mobile.InAir)
        {
            mobile.Velocity.Y -= JumpStrength;
            if (mobile.SupportingEntityId != -1)
            {
                var supportingEnt = store.GetEntityById(mobile.SupportingEntityId);
                var supportingData = supportingEnt.Data;
                if (!supportingEnt.IsNull && supportingData.Has<MobileComponent>())
                    mobile.Velocity += supportingData.Get<MobileComponent>().Velocity;
            }
            mobile.SupportingEntityId = -1;
        }
    }
}
