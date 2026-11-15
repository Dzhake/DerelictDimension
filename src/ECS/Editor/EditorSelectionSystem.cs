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
        bool isSelecting = Input.KeyDown(Key.Mouse1);

        var mouseWorld = Coords.ScreenToWorld(Input.MousePos());
        Entity? firstMatch = null;
        Entity? firstUnselected = null;

        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            var pos = entity.GetWorldPos(out float rotation);
            var boundingBox = EditorSelection.GetBoundingBox(entity.Data);
            boundingBox.Center.Rotate(rotation);
            boundingBox.Center += pos;
            boundingBox.Angle += rotation;
            if (!Collide.IsPointInsideRect(mouseWorld, boundingBox))
                continue;

            firstMatch ??= entity;

            if (!EditorSelection.LastSelectedPids.Contains(entity.Pid))
            {
                firstUnselected = entity;
                break;
            }
        }

        if (firstMatch == null)
        {
            if (isSelecting)
                Deselect();
            else
                Unhover();
            return;
        }

        // All matching entities have already been selected.
        if (firstUnselected == null)
        {
            EditorSelection.LastSelectedPids.Clear();
            firstUnselected = firstMatch;
        }

        if (isSelecting)
            Select(firstUnselected.Value);
        else
            Hover(firstUnselected.Value);
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
