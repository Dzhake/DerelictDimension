using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.Systems;
using Monod.Graphics.ECS.Sprite;
using Monod.TimeModule;
using Monod.Utils.Extensions;
using System;

namespace DerelictDimension.ECS.Ai;

public class BodySystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        float dt = Time.DeltaTime;
        foreach (var entity in ctx.GetQuery(Queries.BodyComponent).Entities)
        {
            var data = entity.Data;
            ref var body = ref data.Get<BodyComponent>();
            ref var sprite = ref data.Ensure<Sprite2D>(ctx.cb);


            var parent = entity.Parent;
            if (parent.IsNull || !parent.HasComponent<PhysicsBodyComponent>()) continue;

            var parentData = parent.Data;
            if (parentData.Has<MortalComponent>() && parentData.Get<MortalComponent>().Dead)
            {
                sprite.Rotation -= body.Rotation;
                body.Rotation = 0;
                return;
            }

            var velocity = parentData.Get<PhysicsBodyComponent>().LinearVelocity;
            if (MathF.Abs(velocity.X) < 0.1f && MathF.Abs(velocity.Y) < 0.1f) continue;

            float oldRotation = body.Rotation;
            body.TimeSinceStartingState = (body.TimeSinceStartingState + dt) % body.RotationTime;
            body.Rotation = MathF.Sin((float)(body.TimeSinceStartingState / body.RotationTime * MathF.PI * 2)) * body.MaxRotation;
            sprite.Rotation += body.Rotation - oldRotation;
        }
    }
}
