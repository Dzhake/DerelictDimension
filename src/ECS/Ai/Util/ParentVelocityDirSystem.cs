using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Utils.Extensions;
using MonoGame.Extended;

namespace DerelictDimension.ECS.Ai.Visual;

public class ParentVelocityDirSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        foreach (var entity in ctx.GetQuery(Queries.ParentVelocityDirTag).Entities)
        {
            var parent = entity.Parent;
            if (parent.IsNull || !parent.HasComponent<PhysicsBodyComponent>()) continue;
            var velocity = parent.GetComponent<PhysicsBodyComponent>().LinearVelocity;
            if (velocity == Vector2.Zero) continue;
            ref var transform = ref entity.Data.Ensure<Transform2D>(ctx.cb);
            transform.Rotation = velocity.ToAngle();
            entity.SetTransformDirtyRecursive();
        }
    }
}
