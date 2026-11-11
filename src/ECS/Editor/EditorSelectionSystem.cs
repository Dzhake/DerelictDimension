using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.InputModule;
using Monod.MathModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Editor;

public class EditorSelectionSystem : SystemBase
{
    public override void Update(SystemCtx ctx)
    {
        Log.Information(EditorSelection.SelectedPid.ToString());
        if (Input.KeyPressed(Key.Mouse1))
        {
            var mouseWorld = Coords.ScreenToWorld(Input.MousePos());
            Entity? firstMatch = null;
            Entity? firstUnselected = null;

            foreach (var entity in ctx.GetQuery(Queries.Sprite2D).Entities)
            {
                ref var sprite = ref entity.GetComponent<Sprite2D>();
                if (sprite.Texture is null || (!sprite.Visible && !EditorSelection.Settings.SelectInvisible)) continue;

                var pos = entity.GetWorldPos(out float rotation);
                if (!Collide.IsPointInsideRect(mouseWorld, pos, rotation + sprite.Rotation, sprite.Texture.Width, sprite.Texture.Height))
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
