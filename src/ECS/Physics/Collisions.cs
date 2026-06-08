using System;

namespace DerelictDimension.ECS.Physics;

public static class Collisions
{
    // Проверка коллизии RectangleF (Actor) и RotatedRectangle (Solid) с расчетом MTV
    public static bool Intersects(RectangleF rect, RotatedRectangle rotatedRect, out Vector2 mtv)
    {
        mtv = Vector2.Zero;

        // 1. Получаем вершины для обоих прямоугольников
        Vector2[] vertsA = {
            new Vector2(rect.X, rect.Y),
            new Vector2(rect.X + rect.Width, rect.Y),
            new Vector2(rect.X + rect.Width, rect.Y + rect.Height),
            new Vector2(rect.X, rect.Y + rect.Height)
        };
        Vector2[] vertsB = rotatedRect.GetVertices();

        // 2. Определяем уникальные оси для теста (2 от AABB, 2 от OBB)
        Vector2[] axes = {
            new Vector2(1, 0), // Горизонтальная ось RectangleF
            new Vector2(0, 1), // Вертикальная ось RectangleF
            GetNormal(vertsB[0], vertsB[1]), // Первая ось RotatedRectangle
            GetNormal(vertsB[1], vertsB[2])  // Вторая ось RotatedRectangle
        };

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        // 3. Тест разделяющих осей (SAT)
        for (int i = 0; i < 4; i++)
        {
            Vector2 axis = axes[i];
            if (axis == Vector2.Zero) continue;

            ProjectVertices(vertsA, axis, out float minA, out float maxA);
            ProjectVertices(vertsB, axis, out float minB, out float maxB);

            // Если проекции не пересекаются — коллизии нет
            if (maxA < minB || maxB < minA)
            {
                return false;
            }

            // Находим глубину проникновения
            float overlap = Math.Min(maxA, maxB) - Math.Max(minA, minB);
            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                smallestAxis = axis;
            }
        }

        // 4. Корректируем направление вектора выталкивания (от Solid к Actor)
        Vector2 rectCenter = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
        Vector2 direction = rectCenter - rotatedRect.Center;
        if (Vector2.Dot(smallestAxis, direction) < 0)
        {
            smallestAxis = -smallestAxis;
        }

        mtv = smallestAxis * minOverlap;
        return true;
    }

    private static Vector2 GetNormal(Vector2 p1, Vector2 p2)
    {
        Vector2 edge = p2 - p1;
        if (edge == Vector2.Zero) return Vector2.Zero;
        Vector2 normal = new Vector2(-edge.Y, edge.X);
        normal.Normalize();
        return normal;
    }

    private static void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
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
