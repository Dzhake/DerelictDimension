public struct AABB
{
    public float CenterX;
    public float CenterY;
    public float HalfWidth;
    public float HalfHeight;

    public AABB(float centerX, float centerY, float halfWidth, float halfHeight)
    {
        CenterX = centerX;
        CenterY = centerY;
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
    }

    // Свойства для работы с Vector2
    public Vector2 Center
    {
        readonly get => new(CenterX, CenterY);
        set { CenterX = value.X; CenterY = value.Y; }
    }

    public Vector2 HalfSize
    {
        readonly get => new(HalfWidth, HalfHeight);
        set { HalfWidth = value.X; HalfHeight = value.Y; }
    }

    public Vector2 Size
    {
        readonly get => new(HalfWidth * 2, HalfHeight * 2);
        set { HalfWidth = value.X / 2; HalfHeight = value.Y / 2; }
    }

    public float X { readonly get => CenterX - HalfWidth; set => CenterX = value + HalfWidth; }
    public float Y { readonly get => CenterY - HalfHeight; set => CenterY = value + HalfHeight; }

    public float Width { readonly get => HalfWidth * 2f; set => HalfWidth = value / 2; }
    public float Height { readonly get => HalfHeight * 2f; set => HalfHeight = value / 2; }

    public readonly Vector2 TopLeft => new(CenterX - HalfWidth, CenterY - HalfHeight);
    public readonly Vector2 TopRight => new(CenterX + HalfWidth, CenterY - HalfHeight);
    public readonly Vector2 BottomLeft => new(CenterX - HalfWidth, CenterY + HalfHeight);
    public readonly Vector2 BottomRight => new(CenterX + HalfWidth, CenterY + HalfHeight);

    public static explicit operator RectangleF(AABB aabb) => new(aabb.X, aabb.Y, aabb.Width, aabb.Height);
    public static explicit operator Rectangle(AABB aabb) => new((int)aabb.X, (int)aabb.Y, (int)aabb.Width, (int)aabb.Height);
}
