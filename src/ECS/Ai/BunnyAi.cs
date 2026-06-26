using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS.Ai;

public record struct BunnyAi : IComponent, IAi
{
    public Vector2 SmallJumpStrength = new(200);
    public Vector2 BigJumpStrength = new(400);
    public int SmallJumpsBeforeBigJump = 3;
    public float TimeBeforeSmallJump = 0.5f;
    public float TimeBeforeBigJump = 2f;
    public float DelayAfterSmallJump = 0;
    public float DelayAfterBigJump = 1f;
    public float RotationSpeedWhileDead = MathF.PI;

    public float TimeGroundedSinceJump;
    public float SmallJumpsDone;

    public BunnyAi()
    {
    }

    public void PostUpdate(Entity entity, EntityStore store, CommandBuffer cb) { }
    public void PreUpdate(Entity entity, EntityStore store, CommandBuffer cb)
    {
        var data = entity.Data;
        if (!Rewind.ShouldUpdateEntity(data)) return;
        if (!data.Has<Transform2D>()) return;
        ref var transform = ref data.Get<Transform2D>();

        if (data.Has<MortalComponent>())
        {
            ref var mortal = ref data.Get<MortalComponent>();
            if (mortal.Dead)
            {
                Rewind.StoreComponentUpdated(entity.Id, ref transform);
                transform.Rotation += RotationSpeedWhileDead * Time.DeltaTime;
            }
        }

        bool hasMobile = data.Has<MobileComponent>();
        bool hasMobileInfo = data.Has<MobileInfoComponent>();
        if (!hasMobile)
        {
            MobileComponent defaultMobile = new();
            Rewind.StoreComponentNonExisting<MobileComponent>(entity.Id);
            cb.AddComponent(data.Id, defaultMobile);
        }

        if (!hasMobileInfo)
        {
            MobileInfoComponent defaultMobileInfo = new(restitution: new(1, 0), restitutionMinimumResultingVelocity: new(0, 1), frictionMult: 3);
            Rewind.StoreComponentNonExisting<MobileInfoComponent>(entity.Id);
            cb.AddComponent(data.Id, defaultMobileInfo);
        }

        if (!hasMobile || !hasMobileInfo) return;

        ref var mobile = ref data.Get<MobileComponent>();

        if (!mobile.Grounded) return;

        Rewind.StoreComponentUpdated(data.Id, ref this);
        TimeGroundedSinceJump += Time.DeltaTime;
        if (SmallJumpsDone >= SmallJumpsBeforeBigJump)
        {
            if (TimeGroundedSinceJump < TimeBeforeBigJump) return;
            SmallJumpsDone -= SmallJumpsBeforeBigJump;
            TimeGroundedSinceJump -= TimeBeforeBigJump + DelayAfterBigJump;
            Rewind.StoreComponentUpdated(data.Id, ref mobile);
            DoBigJump(ref mobile);
        }
        else
        {
            if (TimeGroundedSinceJump < TimeBeforeSmallJump) return;
            TimeGroundedSinceJump -= TimeBeforeSmallJump + DelayAfterSmallJump;
            Rewind.StoreComponentUpdated(data.Id, ref mobile);
            DoSmallJump(ref mobile);
            SmallJumpsDone++;
        }
    }

    private void DoSmallJump(ref MobileComponent mobile)
    {
        //TODO: target nearest player
        mobile.Velocity.X += SmallJumpStrength.X;
        mobile.Velocity.Y -= SmallJumpStrength.Y;
    }

    private void DoBigJump(ref MobileComponent mobile)
    {
        //TODO: target nearest player
        mobile.Velocity.X += BigJumpStrength.X;
        mobile.Velocity.Y -= BigJumpStrength.Y;
    }
}
