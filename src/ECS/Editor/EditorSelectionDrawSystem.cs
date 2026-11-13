using DerelictDimension.ECS.PhysicsModule;
using Monod.ECS.Systems;
using Monod.Graphics.Batching;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionDrawSystem : SystemBase, IDrawer
{
    public override void Update(SystemCtx ctx)
    {
        if (EditorSelection.SelectedPid != -1)
        {
            var entity = ctx.Store.GetEntityByPid(EditorSelection.SelectedPid);

            ctx.drawBatcher.Add(entity, this, new(-2000), ctx);
        }
    }

    public void Draw(Entity entity, SystemCtx ctx)
    {
        var boundingBox = EditorSelection.GetBoundingBox(entity.Data);
        var pos = entity.GetWorldPos(out float rotation);
        boundingBox.Center.Rotate(rotation);
        boundingBox.Center += pos;
        boundingBox.Angle += rotation;

        boundingBox.Center *= Physics.PixelsInMeter;
        boundingBox.Size *= Physics.PixelsInMeter;
        RendererExt.DrawRotRect(boundingBox, Color.Purple * 0.5f);
    }
}
