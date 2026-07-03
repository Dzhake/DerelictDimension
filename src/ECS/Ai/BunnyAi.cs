using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Ai;

public record struct BunnyAi : IComponent, IAi
{


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
        if (!data.Has<Transform2D>() || !data.Has<BunnyAiInfo>()) return;
        ref var transform = ref data.Get<Transform2D>();
        ref var info = ref data.Get<BunnyAiInfo>();

        if (data.Has<MortalComponent>())
        {
            ref var mortal = ref data.Get<MortalComponent>();
            if (mortal.Dead)
            {
                Rewind.StoreComponentUpdated(entity.Id, ref transform);
                transform.Rotation += info.RotationSpeedWhileDead * Time.DeltaTime;
            }
        }


        if (!data.Has<MobileComponent>() || !data.Has<MobileInfoComponent>()) return;

        ref var mobile = ref data.Get<MobileComponent>();

        if (!mobile.Grounded) return;

        Rewind.StoreComponentUpdated(data.Id, ref this);
        TimeGroundedSinceJump += Time.DeltaTime;
        if (SmallJumpsDone >= info.SmallJumpsBeforeBigJump)
        {
            if (TimeGroundedSinceJump < info.TimeBeforeBigJump) return;
            SmallJumpsDone -= info.SmallJumpsBeforeBigJump;
            TimeGroundedSinceJump -= info.TimeBeforeBigJump + info.DelayAfterBigJump;
            Rewind.StoreComponentUpdated(data.Id, ref mobile);
            DoBigJump(ref mobile, ref info);
        }
        else
        {
            if (TimeGroundedSinceJump < info.TimeBeforeSmallJump) return;
            TimeGroundedSinceJump -= info.TimeBeforeSmallJump + info.DelayAfterSmallJump;
            Rewind.StoreComponentUpdated(data.Id, ref mobile);
            DoSmallJump(ref mobile, ref info);
            SmallJumpsDone++;
        }
    }

    private void DoSmallJump(ref MobileComponent mobile, ref BunnyAiInfo info)
    {
        //TODO: target nearest player
        mobile.Velocity.X += info.SmallJumpStrength.X;
        mobile.Velocity.Y -= info.SmallJumpStrength.Y;
    }

    private void DoBigJump(ref MobileComponent mobile, ref BunnyAiInfo info)
    {
        //TODO: target nearest player
        mobile.Velocity.X += info.BigJumpStrength.X;
        mobile.Velocity.Y -= info.BigJumpStrength.Y;
    }
}
