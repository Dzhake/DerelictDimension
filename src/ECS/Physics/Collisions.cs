using System;

namespace DerelictDimension.ECS.Physics;

public static class Collisions
{
    public static bool Intersects(AABB aabb, RotatedRectangle obb, out Vector2 mtv)
    {
        mtv = Vector2.Zero;

        // Половины размеров OBB
        Vector2 obbHalf = new Vector2(obb.Width * 0.5f, obb.Height * 0.5f);

        // Предварительно вычисляем тригонометрию для угла OBB
        float cos = MathF.Cos(obb.Angle);
        float sin = MathF.Sin(obb.Angle);
        float absCos = MathF.Abs(cos);
        float absSin = MathF.Abs(sin);

        // Вектор от центра OBB к центру AABB (уже без лишних сложений/делений!)
        Vector2 centerDiff = new Vector2(aabb.CenterX - obb.Center.X, aabb.CenterY - obb.Center.Y);

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        // --- ОСЬ 1: Глобальная ось X (AABB) ---
        float obbProjX = obbHalf.X * absCos + obbHalf.Y * absSin;
        float overlapX = aabb.HalfWidth + obbProjX - MathF.Abs(centerDiff.X);
        if (overlapX <= 0) return false;

        minOverlap = overlapX;
        smallestAxis = Vector2.UnitX;

        // --- ОСЬ 2: Глобальная ось Y (AABB) ---
        float obbProjY = obbHalf.X * absSin + obbHalf.Y * absCos;
        float overlapY = aabb.HalfHeight + obbProjY - MathF.Abs(centerDiff.Y);
        if (overlapY <= 0) return false;

        if (overlapY < minOverlap)
        {
            minOverlap = overlapY;
            smallestAxis = Vector2.UnitY;
        }

        // Локальные оси OBB
        Vector2 obbAxisX = new Vector2(cos, sin);
        Vector2 obbAxisY = new Vector2(-sin, cos);

        // --- ОСЬ 3: Локальная ось X (OBB) ---
        float distObbX = MathF.Abs(centerDiff.X * cos + centerDiff.Y * sin);
        float aabbProjObbX = aabb.HalfWidth * absCos + aabb.HalfHeight * absSin;
        float overlapObbX = aabbProjObbX + obbHalf.X - distObbX;
        if (overlapObbX <= 0) return false;

        if (overlapObbX < minOverlap)
        {
            minOverlap = overlapObbX;
            smallestAxis = obbAxisX;
        }

        // --- ОСЬ 4: Локальная ось Y (OBB) ---
        float distObbY = MathF.Abs(centerDiff.X * -sin + centerDiff.Y * cos);
        float aabbProjObbY = aabb.HalfWidth * absSin + aabb.HalfHeight * absCos;
        float overlapObbY = aabbProjObbY + obbHalf.Y - distObbY;
        if (overlapObbY <= 0) return false;

        if (overlapObbY < minOverlap)
        {
            minOverlap = overlapObbY;
            smallestAxis = obbAxisY;
        }

        // Определение направления выталкивания
        if (Vector2.Dot(centerDiff, smallestAxis) < 0)
        {
            smallestAxis = -smallestAxis;
        }

        mtv = smallestAxis * minOverlap;
        return true;
    }
}
