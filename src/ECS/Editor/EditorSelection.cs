using DerelictDimension.ECS.PhysicsModule.Components;
using Monod.Graphics.ECS.Sprite;
using Monod.MathModule;
using System.Collections.Generic;

namespace DerelictDimension.ECS.Editor;

public static class EditorSelection
{
    public static long SelectedPid = -1;
    public static HashSet<long> LastSelectedPids = new();

    //todo: serialize
    public static EditorSelectionSettings Settings;

    public static OrientedRect GetBoundingBox(EntityData data)
    {
        if (data.Has<HitboxComponent>()) return data.Get<HitboxComponent>().GetBoundingBox();

        if (data.Has<Sprite2D>())
        {
            ref var sprite = ref data.Get<Sprite2D>();
            if (sprite.Texture is not null)
                return new(0, 0, sprite.Texture.Width / 100f, sprite.Texture.Height / 100f);
        }

        return new(Vector2.Zero, Settings.MinEntitySelectionBox);
    }
}

public struct EditorSelectionSettings
{
    public EditorSelectionMode Mode;
    public bool PreviewSelection;
    public bool SelectInvisible;
    public Vector2 MinEntitySelectionBox = new(0.12f, 0.12f);

    public EditorSelectionSettings() { }
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
    /// Select the entities by clicking a dot at their position.
    /// </summary>
    Position,
}