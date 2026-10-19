using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.ECS;
using Monod.ECS.DefaultComponents;
using MonoGame.Extended;

namespace DerelictDimension.ECS.Ai;

public class ParentVelocityDirSystem : TagSystem<ParentVelocityDirTag>
{
    protected override void OnUpdateGroup()
    {
        foreach (var entity in Query.Entities)
        {
            var parent = entity.Parent;
            if (parent.IsNull || !parent.HasComponent<MobileComponent>()) continue;
            if (!entity.HasComponent<Transform2D>()) continue;
            var velocity = parent.GetComponent<MobileComponent>().Velocity;
            if (velocity == Vector2.Zero) continue;
            entity.GetComponent<Transform2D>().Rotation = velocity.ToAngle();
        }
        base.OnUpdateGroup();
    }
}
