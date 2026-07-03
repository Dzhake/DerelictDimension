using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Ai;

public class BunnySystem : QuerySystem<BunnyAi, BunnyAiInfo>
{
    public ArchetypeQuery<PlayerAi> Players;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        Players = store.Query<PlayerAi>();
    }

    protected override void OnUpdate()
    {
        foreach (var entity in Query.Entities)
        {
            var data = entity.Data;
            if (!Rewind.ShouldUpdateEntity(data)) return;
            if (!data.Has<Transform2D>() || !data.Has<BunnyAiInfo>() || !data.Has<BunnyAi>()) return;
            ref var transform = ref data.Get<Transform2D>();
            ref var info = ref data.Get<BunnyAiInfo>();
            ref var bunny = ref data.Get<BunnyAi>();

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

            Rewind.StoreComponentUpdated(data.Id, ref bunny);
            bunny.TimeGroundedSinceJump += Time.DeltaTime;
            if (bunny.SmallJumpsDone >= info.SmallJumpsBeforeBigJump)
            {
                if (bunny.TimeGroundedSinceJump < info.TimeBeforeBigJump) return;
                bunny.SmallJumpsDone -= info.SmallJumpsBeforeBigJump;
                bunny.TimeGroundedSinceJump -= info.TimeBeforeBigJump + info.DelayAfterBigJump;
                Rewind.StoreComponentUpdated(data.Id, ref mobile);
                Jump(info.BigJumpStrength, ref mobile, ref transform);
            }
            else
            {
                if (bunny.TimeGroundedSinceJump < info.TimeBeforeSmallJump) return;
                bunny.TimeGroundedSinceJump -= info.TimeBeforeSmallJump + info.DelayAfterSmallJump;
                Rewind.StoreComponentUpdated(data.Id, ref mobile);
                Jump(info.SmallJumpStrength, ref mobile, ref transform);
                bunny.SmallJumpsDone++;
            }
        }
    }

    private void Jump(float jumpStrength, ref MobileComponent mobile, ref Transform2D transform)
    {
        Vector2? target = FindClosestPlayer(ref transform.Position);

        if (target.HasValue)
        {
            Vector2 targetVelocity = target.Value - transform.Position;
            targetVelocity.NormalizeSafe();
            mobile.Velocity += targetVelocity * jumpStrength;
        }
        else
        {
            mobile.Velocity.X += jumpStrength;
            mobile.Velocity.Y -= jumpStrength;
        }
    }

    public Vector2? FindClosestPlayer(ref Vector2 position)
    {
        float minDistance = float.MaxValue;
        Vector2? result = null;

        foreach (var playerEnt in Players.Entities)
        {
            var playerData = playerEnt.Data;

            if (playerData.Has<MortalComponent>() && playerData.Get<MortalComponent>().Dead)
            {
                continue;
            }

            if (!playerData.Has<Transform2D>()) continue;
            ref var playerTransform = ref playerData.Get<Transform2D>();

            float distance = Vector2.Distance(position, playerTransform.Position);
            if (distance >= minDistance) continue;

            minDistance = distance;
            result = playerTransform.Position;
        }

        return result;
    }
}
