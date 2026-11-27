using Hexa.NET.ImGui;
using Monod.ECS.Systems;
using Monod.InputModule;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Editor;

public class EditorTreeSystem : SystemBase
{
    public static bool Show;

    // TODO (perf:low) check if there's a better way to implement multiselection, because storing a really long list is probably not the best solution. Afaik IEnumerator allocates on each use, so something like enumerator which returns next entity from the query would be worse here.
    /// <summary>
    /// Entities visible in the tree, in order. Updated on render (probably contains info from the previous frame).
    /// </summary>
    public readonly List<long> VisibleRows = new(1024);

    public bool DoMultiselect;
    public int MultiselectIndex;
    public bool MultiselectIsAdditive;

    public string FilterEntityName = "";
    public string FilterComponentNames = "";
    public string FilterEntityId = "";
    public string FilterEntityPid = "";
    public bool ShowUnmatchingEntities = true;
    public bool ShowUnmatchingParentsEntities = true;

    /// <summary>
    /// Id of the last entity clicked without multiselect active.
    /// </summary>
    public long AnchorPid = -1;
    public int AnchorIndex;

    public override void Update(SystemCtx ctx)
    {
        if (!Show) return;

        VisibleRows.Clear();
        AnchorIndex = -1;

        PushStyles();
        ImGui.Begin("Entity Tree"u8);

        ImGui.SeparatorText("Filters");
        ImGui.InputText("Name", ref FilterEntityName, 100u);
        ImGui.InputText("Id", ref FilterEntityId, 32u);
        ImGui.InputText("Pid", ref FilterEntityPid, 32u);
        ImGui.InputText("Components", ref FilterComponentNames, 100u);

        ImGui.SeparatorText("Tree"u8);
        foreach (var entity in ctx.GetQuery(Queries.All).Entities)
        {
            if (!entity.Parent.IsNull) continue;
            DrawEntity(entity);
        }

        ImGui.End();
        PopStyles();

        if (DoMultiselect)
        {
            DoMultiselect = false;
            ApplyMultiselection();
        }
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

    private void DrawEntity(Entity entity)
    {
        int rowIndex = VisibleRows.Count;
        VisibleRows.Add(entity.Pid);
        if (entity.Pid == AnchorPid) AnchorIndex = rowIndex;

        ImGui.PushID(entity.Id);
        bool pushedColor = false;
        if (EditorSelection.SelectedPids.Contains(entity.Pid))
        {
            pushedColor = true;
            ImGui.PushStyleColor(ImGuiCol.Header, (Color.MediumPurple * 0.5f).PackedValue);
        }
        else if (EditorSelection.HoveredPids.Contains(entity.Pid))
        {
            pushedColor = true;
            ImGui.PushStyleColor(ImGuiCol.Header, (Color.Yellow * 0.5f).PackedValue);
        }

        const ImGuiTreeNodeFlags noChildrenFlags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.Framed;
        const ImGuiTreeNodeFlags hasChildrenFlags = ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;

        ImGui.SetNextItemOpen(true);
        bool open = ImGui.TreeNodeEx(entity.HasName ? entity.Name.value : "", entity.ChildCount == 0 ? noChildrenFlags : hasChildrenFlags);

        if (ImGui.IsItemHovered())
            EditorSelection.HoveredPids.Add(entity.Pid);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            OnEntityClicked(entity, rowIndex);

        if (pushedColor)
            ImGui.PopStyleColor();

        if (open)
        {
            foreach (var child in entity.ChildEntities)
                DrawEntity(child);
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void OnEntityClicked(Entity entity, int rowIndex)
    {
        bool ctrl = Input.IsCtrlDown();
        bool shift = Input.IsShiftDown();
        bool isSelected = EditorSelection.SelectedPids.Contains(entity.Pid);
        var modifier = ctrl ? isSelected ? SelectionModifier.Remove : SelectionModifier.Add : SelectionModifier.Replace;

        if (shift)
        {
            DoMultiselect = true;
            MultiselectIndex = rowIndex;
            MultiselectIsAdditive = ctrl;
            return;
        }

        EditorSelection.Select([entity], modifier);
        AnchorPid = entity.Pid;
    }

    private void ApplyMultiselection()
    {
        long clickedPid = VisibleRows[MultiselectIndex];
        bool isSelected = EditorSelection.SelectedPids.Contains(clickedPid);
        var modifier = MultiselectIsAdditive ? isSelected ? SelectionModifier.Remove : SelectionModifier.Add : SelectionModifier.Replace;

        // Anchor missing (deleted, or never set): degrade to a plain click.
        if (AnchorIndex < 0)
        {
            EditorSelection.Select([clickedPid], modifier);
            AnchorPid = clickedPid;
            return;
        }

        int from = Math.Min(AnchorIndex, MultiselectIndex);
        int to = Math.Max(AnchorIndex, MultiselectIndex);

        EditorSelection.SelectRange(VisibleRows, from, to, modifier);
    }
}
