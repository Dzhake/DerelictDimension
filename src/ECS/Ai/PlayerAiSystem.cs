using DerelictDimension.ECS.PhysicsModule.Components;
using Friflo.Engine.ECS.Systems;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Ai;

public class PlayerAiSystem : QuerySystem<PlayerAi>
{
    protected override void OnUpdate()
    {
        Query.ForEachEntity(UpdatePlayer);
    }

    private void UpdatePlayer(ref PlayerAi player, Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<MobileComponent>() || !data.Has<Transform2D>()) return;

        ref var mobile = ref data.Get<MobileComponent>();
        ref var transform = ref data.Get<Transform2D>();

        Vector2 targetDirection = Vector2.Zero;
        if (Input.KeyDown(Key.A)) targetDirection.X--;
        if (Input.KeyDown(Key.D)) targetDirection.X++;
        if (Input.KeyDown(Key.W)) targetDirection.Y--;
        if (Input.KeyDown(Key.S)) targetDirection.Y++;

        ref var velocity = ref mobile.Velocity;
        targetDirection.NormalizeSafe();
        var targetVelocity = targetDirection * player.TargetVelocity;
        var velChange = targetVelocity - velocity;
        var velChangeLen = velChange.Length();
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
    }
}
