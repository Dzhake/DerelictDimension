using Monod.ECS.DefaultComponents;
using Monod.Graphics.ECS.Sprite;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Editor;

public static class EditorSelection
{
    public static long SelectedPid = -1;
    public static HashSet<long> LastSelectedPids = new();

    //todo: serialize
    public static EditorSelectionSettings Settings;
}

public struct EditorSelectionSettings
{
    public EditorSelectionMode Mode;
    public bool PreviewSelection;
    public bool SelectInvisible;
}

public enum EditorSelectionMode
{
    /// <summary>
    /// Disable the ability to select entities
    /// </summary>
    Disabled,
    /// <summary>
    /// Select the entities by clicking on their <see cref="Sprite2D"/>.
    /// </summary>
    Sprite,
    /// <summary>
    /// Select the entities by clicking a dot at their position (based on the <see cref="Transform2D"/>).
    /// </summary>
    Position,
}