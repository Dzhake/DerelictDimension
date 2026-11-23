using Hexa.NET.ImGui;
using Monod.ECS.Systems;

namespace DerelictDimension.ECS.Editor;

public class EditorTreeSystem : SystemBase
{
    private static byte[] EmptyStr = [];
    public static bool Show;

    public override void Update(SystemCtx ctx)
    {
        if (!Show) return;

        ImGui.Begin("Entity Tree"u8);
        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            if (!entity.Parent.IsNull) continue;
            DrawEntity(entity);
        }
        ImGui.End();
    }

    private static void DrawEntity(Entity entity)
    {
        ImGui.PushID(entity.Id);
        bool pushedColor = false;
        if (EditorSelection.SelectedPids.Contains(entity.Pid))
        {
            pushedColor = true;
            ImGui.PushStyleColor(ImGuiCol.Header, EditorSelectionDrawSystem.SelectedColorV);
        }
        else if (EditorSelection.HoveredPids.Contains(entity.Pid))
        {
            pushedColor = true;
            ImGui.PushStyleColor(ImGuiCol.Header, EditorSelectionDrawSystem.HoveredColorV);
        }

        if (entity.ChildCount == 0)
        {
            ImGui.Text(entity.HasName ? entity.Name.Utf8 : EmptyStr);
        }
        else
        {
            if (ImGui.TreeNodeEx(entity.HasName ? entity.Name.Utf8 : EmptyStr, ImGuiTreeNodeFlags.Framed))
            {
                foreach (var child in entity.ChildEntities)
                {
                    DrawEntity(child);
                }
                ImGui.TreePop();
            }
        }

        if (pushedColor)
        {
            ImGui.PopStyleColor();
        }
        ImGui.PopID();
    }
}
