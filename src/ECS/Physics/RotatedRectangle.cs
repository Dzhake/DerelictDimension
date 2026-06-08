using System;

namespace DerelictDimension.ECS.Physics;

public struct RotatedRectangle
{
    public Vector2 Center;
    public float Width;
    public float Height;
    public float Angle; // В радианах

    public RotatedRectangle(Vector2 center, float width, float height, float angle)
    {
        Center = center;
        Width = width;
        Height = height;
        Angle = angle;
    }

    public RotatedRectangle(float centerX, float centerY, float width, float height, float angle) : this(new(centerX, centerY), width, height, angle) { }

    // Получение 4 вершин прямоугольника в мировых координатах
    public Vector2[] GetVertices()
    {
        Vector2 halfSize = new Vector2(Width / 2f, Height / 2f);
        Vector2[] localVertices =
        {
        new Vector2(-halfSize.X, -halfSize.Y),
        new Vector2(halfSize.X, -halfSize.Y),
        new Vector2(halfSize.X, halfSize.Y),
        new Vector2(-halfSize.X, halfSize.Y)
    };

        float cos = (float)Math.Cos(Angle);
        float sin = (float)Math.Sin(Angle);

        Vector2[] worldVertices = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            float rx = localVertices[i].X * cos - localVertices[i].Y * sin;
            float ry = localVertices[i].X * sin + localVertices[i].Y * cos;
            worldVertices[i] = Center + new Vector2(rx, ry);
        }
        return worldVertices;
    }
}
