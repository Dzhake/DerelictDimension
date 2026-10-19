using DerelictDimension.ECS.PhysicsModule;
using Monod.ECS;
using Monod.ECS.DefaultComponents;
using Monod.InputModule;
using Monod.Utils.Extensions;
using MonoGame.Extended;

namespace DerelictDimension.ECS.Ai;

public class LookAtMouseSystem : TagSystem<LookAtMouseTag>
{
    public CommandBuffer cb;

    protected override void OnAddStore(EntityStore store)
    {
        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
        base.OnAddStore(store);
    }

    protected override void OnRemoveStore(EntityStore store)
    {
        cb.ReturnBuffer();
        base.OnRemoveStore(store);
    }

    protected override void OnUpdateGroup()
    {
        foreach (var entity in Query.Entities)
        {
            ref var transform = ref entity.Data.Ensure<Transform2D>(cb);
            var mouseOffset = Input.MousePos() - (entity.GetWorldPos() * Physics.PixelsInMeter);
            transform.Rotation = mouseOffset.ToAngle();
            transform.PhysicsDirty = true;
            base.OnUpdateGroup();
        }
        cb.Playback();
    }

}
