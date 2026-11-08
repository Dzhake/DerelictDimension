using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;
using Monod.InputModule;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Ai;

public class KickLegSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        ctx.GetQuery(Queries.KickLegComponent).ForEachEntityM(UpdateKickLeg, ctx);
    }

    private void UpdateKickLeg(ref KickLegComponent kickLeg, Entity entity, SystemCtx ctx)
    {
        float dt = Time.DeltaTime;
        ref var kickLegHitbox = ref entity.Data.Ensure<HitboxComponent>(ctx.cb);

        if (kickLeg.KickTimeRemaining > dt)
        {
            if (!kickLegHitbox.Collidable)
            {
                kickLegHitbox.Collidable = true;
                kickLegHitbox.PhysicsDirty = true;
            }
            kickLeg.KickTimeRemaining -= dt;
            return;
        }

        if (kickLegHitbox.Collidable)
        {
            kickLegHitbox.Collidable = false;
            kickLegHitbox.PhysicsDirty = true;
        }

        if (kickLeg.KickCooldownRemaining > dt)
        {
            kickLeg.KickCooldownRemaining -= dt;
            return;
        }
        else
        {
            kickLeg.KickCooldownRemaining = 0;
        }

        ref var kickLegAnim = ref entity.Data.Ensure<AnimatedSpriteComponent>(ctx.cb);
        kickLegAnim.AnimationSpeedMultiplier = 0;
        kickLegAnim.Reset();

        if (kickLeg.KickTimeRemaining > 0)
        {
            EndKick(ref kickLeg, ctx);
            return;
        }

        if (Input.ActionPressed(InputActions.Kick))
        {
            StartKick(ref kickLeg, ref entity.Data.Ensure<AnimatedSpriteComponent>(ctx.cb), ctx);
        }
    }



    private void StartKick(ref KickLegComponent kickLeg, ref AnimatedSpriteComponent kickLegAnim, SystemCtx ctx)
    {
        kickLeg.KickTimeRemaining = kickLeg.KickTimeStarting;
        kickLegAnim.AnimationSpeedMultiplier = 1;
        kickLegAnim.Reset();
        SetLegsVisible(ref kickLeg, false, ctx);
    }

    private void EndKick(ref KickLegComponent kickLeg, SystemCtx ctx)
    {
        kickLeg.KickTimeRemaining = 0;
        kickLeg.KickCooldownRemaining = kickLeg.KickCooldownStarting;
        SetLegsVisible(ref kickLeg, true, ctx);
    }

    private void SetLegsVisible(ref KickLegComponent kickLeg, bool visible, SystemCtx ctx)
    {
        ctx.Store.GetEntityByPid(kickLeg.LeftLegPid).Data.Ensure<Sprite2D>(ctx.cb).Visible = visible;
        ctx.Store.GetEntityByPid(kickLeg.RightLegPid).Data.Ensure<Sprite2D>(ctx.cb).Visible = visible;
    }
}
