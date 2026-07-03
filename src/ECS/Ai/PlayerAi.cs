using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Ai;

public record struct PlayerAiInfo : IComponent
{
    public float TargetXVel = 200;
    public float Acceleration = 2000;
    public float AirAcceleration = 600;
    public float JumpStrength = 500;
    public float JumpBufferTime = 0.1f;
    public float CoyoteTime = 0.2f;

    public PlayerAiInfo()
    {
    }
}

public record struct PlayerAi : IComponent, IAi
{
    public float TimeSinceJumpPressed = -1;
    public float CoyoteTimeLeft;

    public PlayerAi()
    {
    }

    public readonly void PostUpdate(Entity entity, EntityStore store, CommandBuffer _) { }

    public void PreUpdate(Entity entity, EntityStore store, CommandBuffer _)
    {
        var data = entity.Data;
        if (!Rewind.ShouldUpdateEntity(data)) return;
        if (!data.Has<MobileComponent>() || !data.Has<Transform2D>() || !data.Has<PlayerAiInfo>()) return;

        ref var mobile = ref data.Get<MobileComponent>();
        ref var transform = ref data.Get<Transform2D>();
        ref var info = ref data.Get<PlayerAiInfo>();

        Rewind.StoreComponentUpdated(entity.Id, ref transform);
        Rewind.StoreComponentUpdated(entity.Id, ref mobile);
        Rewind.StoreComponentUpdated(entity.Id, ref this);

        bool left = Input.KeyDown(Key.Left);
        bool right = Input.KeyDown(Key.Right);

        ref float xvel = ref mobile.Velocity.X;
        float xAccel = mobile.InAir ? info.AirAcceleration : info.Acceleration;
        xAccel *= Time.DeltaTime;

        if (mobile.Grounded)
        {
            var supportingEnt = store.GetEntityByPid(mobile.SupportingEntityPid);
            var supportingData = supportingEnt.Data;
            if (!supportingEnt.IsNull && supportingData.Has<SupportComponent>())
            {
                ref var support = ref supportingData.Get<SupportComponent>();
                xAccel *= support.AccelerationMult;
            }

            CoyoteTimeLeft = info.CoyoteTime;
        }
        else
        {
            CoyoteTimeLeft -= Time.DeltaTime;
        }

        if (left && !right)
        {
            float targetX = -info.TargetXVel;
            transform.FlipX = true;
            if (targetX < xvel)
            {
                xvel -= xAccel;
                if (xvel < targetX) xvel = targetX;
            }
        }
        else if (right && !left)
        {
            float targetX = info.TargetXVel;
            transform.FlipX = false;
            if (targetX > xvel)
            {
                xvel += xAccel;
                if (xvel > targetX) xvel = targetX;
            }
        }

        if (TimeSinceJumpPressed >= 0)
        {
            if (TimeSinceJumpPressed <= info.JumpBufferTime && mobile.Grounded)
            {
                Jump(store, ref mobile, ref info);
                TimeSinceJumpPressed = -1;
                CoyoteTimeLeft = -1;
            }

            TimeSinceJumpPressed += Time.DeltaTime;
        }

        if (Input.KeyPressed(Key.Space))
        {
            if (mobile.Grounded || CoyoteTimeLeft >= 0)
            {
                Jump(store, ref mobile, ref info);
                TimeSinceJumpPressed = -1;
                CoyoteTimeLeft = -1;
            }
            else
            {
                TimeSinceJumpPressed = 0;
            }
        }

    }

    private static void Jump(EntityStore store, ref MobileComponent mobile, ref PlayerAiInfo info)
    {
        mobile.Velocity.Y -= info.JumpStrength;
        if (mobile.Grounded)
        {
            var supportingEnt = store.GetEntityByPid(mobile.SupportingEntityPid);
            var supportingData = supportingEnt.Data;
            if (!supportingEnt.IsNull && supportingData.Has<MobileComponent>())
                mobile.Velocity += supportingData.Get<MobileComponent>().Velocity;
        }
        mobile.SupportingEntityPid = -1;
    }
}
