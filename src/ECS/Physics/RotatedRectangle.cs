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

    // Проверка пересечения методом SAT с возвратом вектора выталкивания (mtv)
    public bool Intersects(RotatedRectangle other, out Vector2 mtv)
    {
        mtv = Vector2.Zero;

        Vector2[] vertsA = this.GetVertices();
        Vector2[] vertsB = other.GetVertices();

        // У прямоугольников всего 4 уникальные оси (перпендикуляры к граням)
        Vector2[] axes =
        {
        GetNormal(vertsA[0], vertsA[1]),
        GetNormal(vertsA[1], vertsA[2]),
        GetNormal(vertsB[0], vertsB[1]),
        GetNormal(vertsB[1], vertsB[2])
    };

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        for (int i = 0; i < 4; i++)
        {
            Vector2 axis = axes[i];
            if (axis == Vector2.Zero) continue;

            ProjectVertices(vertsA, axis, out float minA, out float maxA);
            ProjectVertices(vertsB, axis, out float minB, out float maxB);

            // Если проекции не пересекаются, коллизии нет (нашли разделяющую ось)
            if (maxA < minB || maxB < minA)
            {
                return false;
            }

            // Вычисляем величину перекрытия проекций
            float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                smallestAxis = axis;
            }
        }

        // Гарантируем, что вектор выталкивания направлен от 'other' к 'this'
        Vector2 direction = this.Center - other.Center;
        if (Vector2.Dot(smallestAxis, direction) < 0)
        {
            smallestAxis = -smallestAxis;
        }

        mtv = smallestAxis * minOverlap;
        return true;
    }

    private Vector2 GetNormal(Vector2 p1, Vector2 p2)
    {
        Vector2 edge = p2 - p1;
        if (edge == Vector2.Zero) return Vector2.Zero;
        Vector2 normal = new Vector2(-edge.Y, edge.X);
        normal.Normalize();
        return normal;
    }

    private void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
    {
        min = Vector2.Dot(vertices[0], axis);
        max = min;
        for (int i = 1; i < vertices.Length; i++)
        {
            float proj = Vector2.Dot(vertices[i], axis);
            if (proj < min) min = proj;
            if (proj > max) max = proj;
        }
    }
}
