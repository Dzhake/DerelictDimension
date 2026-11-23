using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.InputModule;
using Monod.MathModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

/// <summary>
/// System for selecting the entities on the screen with your mouse.
/// </summary>
public class EditorSelectionSystem : SystemBase
{
    public bool IsDragging;
    public Vector2 ClickWorldPos;

    public override void OnCtxChanged(SystemCtx? oldCtx, SystemCtx ctx)
    {
        if (oldCtx != null)
            ctx.Store.OnEntityDelete -= OnEntityDeleted;

        ctx.Store.OnEntityDelete += OnEntityDeleted;
    }

    private void OnEntityDeleted(EntityDelete delete)
    {
        EditorSelection.SelectedPids.Remove(delete.Entity.Pid);
        EditorSelection.HoveredPids.Remove(delete.Entity.Pid);
        EditorSelection.MarqueeSelected.Remove(delete.Entity.Pid);
    }

    public override void Update(SystemCtx ctx)
    {
        var settings = EditorSelection.Settings;

        if (settings.SelectMode == EditorSelectionMode.Disabled)
        {
            EditorSelection.ClearHover();
            EndDrag();
            return;
        }

        var mouseScreen = Input.MousePos();
        var mouseWorld = Coords.ScreenToWorld(mouseScreen);
        bool released = Input.KeyReleased(Key.Mouse1);

        // Missed the release (skipped frame, focus loss, etc.): abort the drag.
        if (IsDragging && !Input.KeyDown(Key.Mouse1) && !released)
        {
            EndDrag();
            return;
        }

        if (!IsDragging && Input.KeyPressed(Key.Mouse1))
        {
            BeginPress(ctx, mouseWorld);
        }
        else if (IsDragging && !EditorSelection.IsBoxSelecting && ExceedsDragThreshold(mouseScreen, settings.DragThreshold))
        {
            EditorSelection.IsBoxSelecting = true;
        }


        if (!EditorSelection.IsBoxSelecting)
        {
            UpdateHover(ctx, mouseWorld);
            if (released)
                CommitClick(ctx, mouseWorld);
        }
        else
        {
            UpdateMarquee(ctx, mouseWorld);
            if (released)
                CommitMarquee(ctx, mouseWorld);
        }
    }



    private void BeginPress(SystemCtx ctx, Vector2 mouseWorld)
    {
        ClickWorldPos = mouseWorld;
        IsDragging = true;
    }

    private static void UpdateHover(SystemCtx ctx, Vector2 mouseWorld)
    {
        EditorSelection.HoveredPids.Clear();

        if (Input.IsMouseCaptured()) return;

        long target = PickClickTarget(ctx, mouseWorld);
        if (target != -1)
            EditorSelection.HoveredPids.Add(target);
    }

    private void CommitClick(SystemCtx ctx, Vector2 mouseWorld)
    {
        long target = PickClickTarget(ctx, mouseWorld);
        EndDrag();

        if (target == -1)
        {
            // When clicking empty space, plain click and intersect clear the selection, add and subtract are no-ops.
            var modifier = GetModifier();
            if (modifier is SelectionModifier.Replace or SelectionModifier.Intersect)
            {
                EditorSelection.ClearSelection();
                EditorSelection.ResetClickCycle();
            }
            return;
        }

        EditorSelection.Select([target], GetModifier());
        EditorSelection.ClickCyclePids.Add(target);
    }

    private void UpdateMarquee(SystemCtx ctx, Vector2 mouseWorld)
    {
        EditorSelection.MarqueeStart = ClickWorldPos;
        EditorSelection.MarqueeEnd = mouseWorld;

        var marquee = OrientedRect.RectBetween(ClickWorldPos, mouseWorld);

        EditorSelection.MarqueeSelected.Clear();
        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            if (Collide.RectsIntersect(marquee, GetWorldBoundingBox(entity)))
                EditorSelection.MarqueeSelected.Add(entity.Pid);
        }
    }

    private void CommitMarquee(SystemCtx ctx, Vector2 mouseWorld)
    {
        var modifier = GetModifier();
        EditorSelection.Select(EditorSelection.MarqueeSelected, modifier);

        EndDrag();
        EditorSelection.ResetClickCycle();
    }

    private void EndDrag()
    {
        IsDragging = false;
        EditorSelection.IsBoxSelecting = false;
        EditorSelection.MarqueeSelected.Clear();
    }


    private static SelectionModifier GetModifier()
    {
        bool ctrl = Input.IsCtrlDown();
        bool shift = Input.IsShiftDown();

        if (ctrl && shift) return SelectionModifier.Intersect;
        if (ctrl) return SelectionModifier.Remove;
        if (shift) return SelectionModifier.Add;
        return SelectionModifier.Replace;
    }

    private bool ExceedsDragThreshold(Vector2 mouseScreen, float thresholdPixels)
    {
        var movement = Coords.ScreenToWorld(mouseScreen) - ClickWorldPos;
        return movement.LengthSquared() > thresholdPixels * thresholdPixels;
    }

    /// <summary>
    /// Closest entity under the cursor that the click cycle hasn't visited yet. Returns -1 when the cursor is over nothing.
    /// </summary>
    private static long PickClickTarget(SystemCtx ctx, Vector2 worldPos)
    {
        long closest = -1;
        float closestDist = float.MaxValue;
        long closestNew = -1;
        float closestNewDist = float.MaxValue;

        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            var worldBox = GetWorldBoundingBox(entity);
            if (!Collide.IsPointInsideRect(worldPos, worldBox))
                continue;

            float dist = (worldPos - worldBox.Center).LengthSquared();

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = entity.Pid;
            }

            if (!EditorSelection.ClickCyclePids.Contains(entity.Pid) && dist < closestNewDist)
            {
                closestNewDist = dist;
                closestNew = entity.Pid;
            }
        }

        // Everything under the cursor was already clicked through: restart the cycle.
        if (closest != -1 && closestNew == -1)
            EditorSelection.ClickCyclePids.Clear();

        return closestNew != -1 ? closestNew : closest;
    }

    private static OrientedRect GetWorldBoundingBox(Entity entity)
    {
        var pos = entity.GetWorldPos(out float rotation);
        var boundingBox = EditorSelection.GetBoundingBox(entity.Data);
        boundingBox.Center.Rotate(rotation);
        boundingBox.Center += pos;
        boundingBox.Angle += rotation;
        return boundingBox;
    }
}