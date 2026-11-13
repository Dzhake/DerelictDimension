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
        if (Input.KeyPressed(Key.Mouse1))
        {
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
                Deselect();
                return;
            }

            // All matching entities have already been selected.
            if (firstUnselected == null)
            {
                EditorSelection.LastSelectedPids.Clear();
                firstUnselected = firstMatch;
            }

            EditorSelection.SelectedPid = firstUnselected.Value.Pid;
            EditorSelection.LastSelectedPids.Add(firstUnselected.Value.Pid);
        }
    }

    private void Deselect()
    {
        EditorSelection.SelectedPid = -1;
        EditorSelection.LastSelectedPids.Clear();
    }
}
