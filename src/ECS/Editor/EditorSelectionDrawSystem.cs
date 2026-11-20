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
        foreach (long pid in EditorSelection.SelectedPids)
        {
            var entity = ctx.Store.GetEntityByPid(pid);
            ctx.drawBatcher.Add(entity, DrawSelection, SelectedDepth with { PartOffset = pid });
        }

        foreach (long pid in EditorSelection.HoveredPids)
        {
            var entity = ctx.Store.GetEntityByPid(pid);
            ctx.drawBatcher.Add(entity, DrawHover, HoveredDepth with { PartOffset = pid });
        }

        foreach (long pid in EditorSelection.MarqueeSelected)
        {
            var entity = ctx.Store.GetEntityByPid(pid);
            ctx.drawBatcher.Add(entity, DrawPreview, HoveredDepth with { PartOffset = pid });
        }

        if (EditorSelection.IsBoxSelecting)
        {
            ctx.drawBatcher.Add(default, DrawMarquee, SelectedDepth);
        }
    }

    public static void DrawMarquee(Entity entity, SystemCtx ctx)
    {
        Renderer.DrawHollowRect(MathM.RectangleBetween(EditorSelection.MarqueeStart * Physics.PixelsInMeter, EditorSelection.MarqueeEnd * Physics.PixelsInMeter), Color.White, 3);
    }

    public static void DrawSelection(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawRotRect(boundingBox, Color.Purple * 0.3f);
        Renderer.DrawHollowRotRect(boundingBox, Color.MediumPurple * 0.5f, 4);
    }

    public static void DrawHover(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawHollowRotRect(boundingBox, Color.Yellow * 0.5f);
    }

    public static void DrawPreview(Entity entity, SystemCtx ctx)
    {
        var boundingBox = GetBoundingBox(entity);
        Renderer.DrawRotRect(boundingBox, Color.YellowGreen * 0.2f);
        Renderer.DrawHollowRotRect(boundingBox, Color.Yellow * 0.2f);
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

}
