using DerelictDimension.ECS.Drawing;
using DerelictDimension.ECS.Physics.Components;
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
            var velocity = parent.GetComponent<MobileComponent>().Velocity.Length();
            ref var animatedSprite = ref entity.GetComponent<AnimatedSpriteComponent>();
            if (velocity == 0)
            {
                animatedSprite.FrameRemainder = 0;
                animatedSprite.FrameNumber = 0;
                animatedSprite.InvalidateTexture = true;
            }
            //animatedSprite.AnimationSpeedMultiplier = velocity / PlayerAi.NormalWalkingSpeed;
        }
    }
}
