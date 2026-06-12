namespace DerelictDimension.ECS;

[ComponentKey("SpriteComponent")]
[ComponentSymbol("S")]
public record struct SpriteComponent : IComponent
{
    public Vector2 Offset = Vector2.Zero;
    public Vector2? Origin;
    /// <summary>
    /// Rotation (in radians) around <see cref="Origin"/>.
    /// </summary>
    public float Rotation;
    public SpriteEffects spriteEffects;
    public Color ColorScale = Color.White;
    public Texture2D? Texture;

    public SpriteComponent()
    {
    }
}
