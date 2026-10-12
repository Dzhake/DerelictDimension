namespace DerelictDimension.ECS.Physics;

public record struct OrientedRectangle
{
    public Vector2 Center;
    public float Width;
    public float Height;
    /// <summary>
    /// Angle in radians.
    /// </summary>
    public float Angle;

    public OrientedRectangle(Vector2 center, float width, float height, float angle)
    {
        Center = center;
        Width = width;
        Height = height;
        Angle = angle;
    }

    public OrientedRectangle(float centerX, float centerY, float width, float height, float angle) : this(new(centerX, centerY), width, height, angle) { }

    public override string ToString() => $"Center: {Center}; Width: {Width}; Height: {Height}; Angle: {Angle}";
}
