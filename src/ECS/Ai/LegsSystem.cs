using Box2D.NET;
using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.PhysicsModule.Components;
using DerelictDimension.Utils.Extensions;
using Monod.ECS;

namespace DerelictDimension.ECS.Ai;


public class LegsSystem : TagSystem<LegsTag>
{
    protected override void OnUpdateGroup()
    {
        foreach (var entity in Query.Entities)
        {
            var parent = entity.Parent;
            if (parent.IsNull) return;
            if (!entity.HasComponent<AnimatedSpriteComponent>()) continue;
            var velocity = B2Bodies.b2Body_GetLinearVelocity(parent.GetComponent<PhysicsBodyComponent>().BodyId).ToXNA().Length();
            if (velocity != 0)
                continue;
            ref var animatedSprite = ref entity.GetComponent<AnimatedSpriteComponent>();
            animatedSprite.FrameRemainder = 0;
            animatedSprite.FrameNumber = 0;
            animatedSprite.InvalidateTexture = true;
        }
    }
}
