using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.InputModule;
using Monod.MathModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        bool isSelecting = Input.KeyPressed(Key.Mouse1);

        var mouseWorld = Coords.ScreenToWorld(Input.MousePos());
        (Entity ent, float dist)? closestMatch = null;
        (Entity ent, float dist)? closestUnselected = null;


        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            var pos = entity.GetWorldPos(out float rotation);
            var boundingBox = EditorSelection.GetBoundingBox(entity.Data);
            boundingBox.Center.Rotate(rotation);
            boundingBox.Center += pos;
            boundingBox.Angle += rotation;
            if (!Collide.IsPointInsideRect(mouseWorld, boundingBox))
                continue;

            float dist = (mouseWorld - pos).Length();

            if (closestMatch is null || dist < closestMatch.Value.dist)
            {
                closestMatch = (entity, dist);
            }

            if (!EditorSelection.LastSelectedPids.Contains(entity.Pid) && (closestUnselected is null || dist < closestUnselected.Value.dist))
            {
                closestUnselected = (entity, dist);
            }
        }

        if (closestMatch == null)
        {
            if (isSelecting)
                Deselect();
            else
                Unhover();
            return;
        }

        // All matching entities have already been selected.
        if (closestUnselected == null)
        {
            EditorSelection.LastSelectedPids.Clear();
            closestUnselected = closestMatch;
        }

        if (isSelecting)
            Select(closestUnselected.Value.ent);
        else
            Hover(closestUnselected.Value.ent);
    }


    private static void Select(Entity firstUnselected)
    {
        EditorSelection.SelectedPid = firstUnselected.Pid;
        EditorSelection.LastSelectedPids.Add(firstUnselected.Pid);
    }

    private void Deselect()
    {
        EditorSelection.SelectedPid = -1;
        EditorSelection.LastSelectedPids.Clear();
    }

    private void Hover(Entity firstUnselected)
    {
        EditorSelection.HoveredPid = firstUnselected.Pid;
    }

    private void Unhover()
    {
        EditorSelection.HoveredPid = -1;
    }
}
