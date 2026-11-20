using DerelictDimension.ECS.PhysicsModule;
using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;
using System;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Editor;

/// <summary>
/// How a batch of entities is combined with the current selection.
/// Hotkeys: none = Replace, Shift = Add, Ctrl = Remove, Ctrl+Shift = Intersect.
/// </summary>
public enum SelectionModifier
{
    Replace,
    Add,
    Remove,
    Intersect,
}

public enum EditorSelectionMode
{
    /// <summary>
    /// Disable the ability to select entities
    /// </summary>
    Disabled,
    /// <summary>
    /// Select the entities by clicking a dot at their position.
    /// </summary>
    Position,
    /// <summary>
    /// Select the entities by clicking on their hitbox. Fallbacks to <see cref="Position"/>.
    /// </summary>
    Hitbox,
    /// <summary>
    /// Select the entities by clicking on their <see cref="Sprite2D"/>. Fallbacks to <see cref="Hitbox"/>.
    /// </summary>
    Sprite,
}

public static class EditorSelection
{
    /// <summary>
    /// Entities currently selected.
    /// </summary>
    public static HashSet<long> SelectedPids = new();

    /// <summary>
    /// Entities currently hovered.
    /// </summary>
    public static HashSet<long> HoveredPids = new();

    /// <summary>
    /// Entities inside the active box-select rectangle. Only populated while <see cref="IsBoxSelecting"/> is <see langword="true"/>.
    /// </summary>
    public static HashSet<long> MarqueeSelected = new();

    /// <summary>
    /// True while the user is box-selecting (drag passed the pixel threshold).
    /// </summary>
    public static bool IsBoxSelecting;

    /// <summary>
    /// Marquee corners in world space. Only valid while <see cref="IsBoxSelecting"/>.
    /// </summary>
    public static Vector2 MarqueeStart;
    public static Vector2 MarqueeEnd;

    /// <summary>
    /// Entities hovered by the mouse, used for the stacked-entities cycling purposes.
    /// </summary>
    public static HashSet<long> ClickCyclePids = new();

    /// <summary>
    /// Fired whenever <see cref="SelectedPids"/> changes through the methods below.
    /// </summary>
    public static event Action? SelectionChanged;

    //todo: serialize
    public static EditorSelectionSettings Settings = new();



    public static bool IsSelected(long pid) => SelectedPids.Contains(pid);
    public static bool IsSelected(Entity entity) => SelectedPids.Contains(entity.Pid);
    public static bool IsHovered(long pid) => HoveredPids.Contains(pid);
    public static bool IsHovered(Entity entity) => HoveredPids.Contains(entity.Pid);

    public static void Select(IEnumerable<Entity> entities, SelectionModifier modifier = SelectionModifier.Replace)
        => Select(ToPids(entities), modifier);

    public static void Select(IEnumerable<long> pids, SelectionModifier modifier = SelectionModifier.Replace)
    {
        switch (modifier)
        {
        case SelectionModifier.Replace:
            SelectedPids.Clear();
            SelectedPids.UnionWith(pids);
            break;

        case SelectionModifier.Add:
            SelectedPids.UnionWith(pids);
            break;

        case SelectionModifier.Remove:
            SelectedPids.ExceptWith(pids);
            break;

        case SelectionModifier.Intersect:
            SelectedPids.IntersectWith(pids);
            break;
        }

        SelectionChanged?.Invoke();
    }

    public static void AddToSelection(IEnumerable<Entity> entities) => Select(entities, SelectionModifier.Add);
    public static void AddToSelection(IEnumerable<long> pids) => Select(pids, SelectionModifier.Add);

    public static void RemoveFromSelection(IEnumerable<Entity> entities) => Select(entities, SelectionModifier.Remove);
    public static void RemoveFromSelection(IEnumerable<long> pids) => Select(pids, SelectionModifier.Remove);

    public static void IntersectSelection(IEnumerable<Entity> entities) => Select(entities, SelectionModifier.Intersect);
    public static void IntersectSelection(IEnumerable<long> pids) => Select(pids, SelectionModifier.Intersect);

    public static void ClearSelection()
    {
        if (SelectedPids.Count == 0)
            return;

        SelectedPids.Clear();
        SelectionChanged?.Invoke();
    }

    public static void ClearHover() => HoveredPids.Clear();

    /// <summary>
    /// Forget which stacked entities were already clicked through.
    /// </summary>
    public static void ResetClickCycle() => ClickCyclePids.Clear();

    private static IEnumerable<long> ToPids(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
            yield return entity.Pid;
    }

    public static OrientedRect GetBoundingBox(EntityData data)
    {
        if (Settings.SelectMode >= EditorSelectionMode.Hitbox && data.Has<HitboxComponent>())
        {
            return data.Get<HitboxComponent>().GetBoundingBox();
        }

        if (Settings.SelectMode >= EditorSelectionMode.Sprite && data.Has<Sprite2D>())
        {
            ref var sprite = ref data.Get<Sprite2D>();
            if (sprite.Texture is not null)
                return new(0, 0, sprite.Texture.Width / Physics.PixelsInMeter, sprite.Texture.Height / Physics.PixelsInMeter);
        }

        if (Settings.SelectMode == EditorSelectionMode.Position)
            return new(Vector2.Zero, Settings.MinEntitySelectionBox);

        return new(Vector2.Zero, Vector2.Zero);
    }
}

public struct EditorSelectionSettings
{
    public EditorSelectionMode SelectMode = EditorSelectionMode.Sprite;

    /// <summary> Whether entities inside the box-select rectangle get a preview outline while dragging. </summary>
    public bool PreviewSelection = true;

    public bool SelectInvisible;

    public Vector2 MinEntitySelectionBox = new(0.12f, 0.12f);

    /// <summary>
    /// World-space distance the mouse must travel while the button is held before a click turns into a box select.
    /// summary>
    public float DragThreshold = 0.1f;

    public EditorSelectionSettings() { }
}