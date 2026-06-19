namespace DerelictDimension.ECS.Drawing;

public record struct SingleSpriteComponent : IComponent
{
    public Vector2 Offset = Vector2.Zero;
    public Vector2? Origin;
    /// <summary>
    /// Rotation (in radians) around <see cref="Origin"/>.
    /// </summary>
    public float Rotation;
    public SpriteEffects spriteEffects;
    public Color ColorScale = Color.White;
    public string TexturePath;
    public Texture2D? Texture;

    public SingleSpriteComponent(string texturePath)
    {
        TexturePath = texturePath;
    }
}
