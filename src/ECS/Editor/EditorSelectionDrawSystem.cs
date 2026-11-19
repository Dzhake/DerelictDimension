using DerelictDimension.ECS.PhysicsModule;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.MathModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionDrawSystem : SystemBase
{
    public static RenderDepth SelectedDepth = new(-2000);
    public static RenderDepth HoveredDepth = new(-2005);

    public override void Update(SystemCtx ctx)
    {
        if (EditorSelection.SelectedPid != -1)
        {
            var entity = ctx.Store.GetEntityByPid(EditorSelection.SelectedPid);

            ctx.drawBatcher.Add(entity, DrawSelection, SelectedDepth);
        }

        if (EditorSelection.HoveredPid != -1)
        {
            var entity = ctx.Store.GetEntityByPid(EditorSelection.HoveredPid);

            ctx.drawBatcher.Add(entity, DrawHover, HoveredDepth);
        }
    }

    public static void DrawSelection(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawRotRect(boundingBox, Color.Purple * 0.5f);
        Renderer.DrawHollowRotRect(boundingBox, Color.LightPink, 2);
    }

    private static OrientedRect GetBoundingBox(Entity entity)
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

    public static void DrawHover(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawHollowRotRect(boundingBox, Color.Yellow * 0.5f);
    }
}
