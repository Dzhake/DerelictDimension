using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS.Systems;

namespace DerelictDimension.ECS.Ai;


public class LegsSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        foreach (var entity in ctx.GetQuery(Queries.LegsTag).Entities)
        {
            var parent = entity.Parent;
            if (parent.IsNull) continue;
            if (!entity.HasComponent<AnimatedSpriteComponent>()) continue;
            float velocity = parent.GetComponent<PhysicsBodyComponent>().LinearVelocity.Length();
            if (velocity > 0.1f)
                continue;
            ref var animatedSprite = ref entity.GetComponent<AnimatedSpriteComponent>();
            animatedSprite.Reset();
        }
    }
}
