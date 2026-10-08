using Monod.ECS;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.Utils.Extensions;
using MonoGame.Extended;

namespace DerelictDimension.ECS.Ai;

public class LookAtMouseSystem : TagSystem<LookAtMouseTag>
{
    protected override void OnUpdateGroup()
    {
        foreach (var entity in Query.Entities)
        {
            if (!entity.HasComponent<Transform2D>()) return;
            ref var transform = ref entity.GetComponent<Transform2D>();
            var mouseOffset = Input.MousePos() - entity.GetWorldPos();
            transform.Rotation = mouseOffset.ToAngle();
            base.OnUpdateGroup();
        }
    }

}
