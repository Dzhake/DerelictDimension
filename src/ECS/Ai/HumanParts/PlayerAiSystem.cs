using Box2D.NET;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.Utils.Extensions;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.InputModule;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Ai;

public class PlayerAiSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        ctx.GetQuery(Queries.PlayerAi).ForEachEntityM(UpdatePlayer, ctx);
    }

    private void UpdatePlayer(ref PlayerAi player, Entity entity, SystemCtx ctx)
    {
        var data = entity.Data;
        ref var physicsBody = ref data.Ensure<PhysicsBodyComponent>(ctx.cb);
        if (!B2Worlds.b2Body_IsValid(physicsBody.BodyId)) return;
        ref var transform = ref data.Ensure<Transform2D>(ctx.cb);

        var velocity = B2Bodies.b2Body_GetLinearVelocity(physicsBody.BodyId).ToXNA();

        var targetDirection = Vector2.Zero;
        if (Input.ActionDown(InputActions.MoveLeft)) targetDirection.X--;
        if (Input.ActionDown(InputActions.MoveRight)) targetDirection.X++;
        if (Input.ActionDown(InputActions.MoveUp)) targetDirection.Y--;
        if (Input.ActionDown(InputActions.MoveDown)) targetDirection.Y++;

        targetDirection.NormalizeSafe();
        var targetVelocity = targetDirection * player.TargetVelocity;
        var velChange = targetVelocity - velocity;
        float velChangeLen = velChange.Length();
        if (velChangeLen < player.Acceleration * Time.DeltaTime)
        {
            velocity = targetVelocity;
        }
        else
        {
            velChange.NormalizeSafe();
            velChange *= player.Acceleration * Time.DeltaTime;
            velocity += velChange;
        }

        B2Bodies.b2Body_SetLinearVelocity(physicsBody.BodyId, velocity.ToB2());
    }
}
