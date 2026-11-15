using DerelictDimension.ECS.PhysicsModule;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionDrawSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        if (EditorSelection.SelectedPid != -1)
        {
            var entity = ctx.Store.GetEntityByPid(EditorSelection.SelectedPid);

            ctx.drawBatcher.Add(entity, DrawSelection, new(-2000), ctx);
        }

        if (EditorSelection.HoveredPid != -1)
        {
            var entity = ctx.Store.GetEntityByPid(EditorSelection.HoveredPid);

            ctx.drawBatcher.Add(entity, DrawHover, new(-2005), ctx);
        }
    }

    public void DrawSelection(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawRotRect(boundingBox, Color.Purple * 0.5f);
        Renderer.DrawHollowRotRect(boundingBox, Color.LightPink, 2);
    }

    private static Monod.MathModule.OrientedRect GetBoundingBox(Entity entity)
    {
        var boundingBox = EditorSelection.GetBoundingBox(entity.Data);
        var pos = entity.GetWorldPos(out float rotation);
        boundingBox.Center.Rotate(rotation);
        boundingBox.Center += pos;
        boundingBox.Angle += rotation;

        boundingBox.Center *= Physics.PixelsInMeter;
        boundingBox.Size *= Physics.PixelsInMeter;
        return boundingBox;
    }

    public void DrawHover(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawHollowRotRect(boundingBox, Color.Yellow * 0.5f);
    }
}
