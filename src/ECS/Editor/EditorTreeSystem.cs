using Hexa.NET.ImGui;
using Monod.ECS.Systems;

namespace DerelictDimension.ECS.Editor;

public class EditorTreeSystem : SystemBase
{
    public static bool Show;


    public override void Update(SystemCtx ctx)
    {
        if (!Show) return;

        PushStyles();
        ImGui.Begin("Entity Tree"u8);

        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            if (!entity.Parent.IsNull) continue;
            DrawEntity(entity);
        }

        ImGui.End();
        PopStyles();
    }

    public void PushStyles()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(4, 1));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(8, 1));
    }

    public void PopStyles()
    {
        ImGui.PopStyleVar(2);
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

        const ImGuiTreeNodeFlags noChildrenFlags = ImGuiTreeNodeFlags.Leaf;
        const ImGuiTreeNodeFlags hasChildrenFlags = ImGuiTreeNodeFlags.Framed;
        if (ImGui.TreeNodeEx(entity.HasName ? entity.Name.value : "", entity.ChildCount == 0 ? noChildrenFlags : hasChildrenFlags))
        {
            foreach (var child in entity.ChildEntities)
            {
                DrawEntity(child);
            }
            ImGui.TreePop();
        }

        if (pushedColor)
        {
            ImGui.PopStyleColor();
        }
        ImGui.PopID();
    }
}
