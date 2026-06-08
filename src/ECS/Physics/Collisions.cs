using System;

namespace DerelictDimension.ECS.Physics;

public static class Collisions
{
    public static bool Intersects(RectangleF aabb, RotatedRectangle obb, out Vector2 mtv)
    {
        mtv = Vector2.Zero;

        // 1. Вычисляем центр и половины размеров AABB
        // (ОПТИМИЗАЦИЯ: В будущем лучше хранить эти данные прямо в структуре AABB)
        Vector2 aabbHalf = new Vector2(aabb.Width * 0.5f, aabb.Height * 0.5f);
        Vector2 aabbCenter = new Vector2(aabb.X + aabbHalf.X, aabb.Y + aabbHalf.Y);

        // 2. Половины размеров OBB
        Vector2 obbHalf = new Vector2(obb.Width * 0.5f, obb.Height * 0.5f);

        // 3. Предварительно вычисляем тригонометрию для угла OBB (в радианах)
        float cos = MathF.Cos(obb.Angle);
        float sin = MathF.Sin(obb.Angle);
        float absCos = MathF.Abs(cos);
        float absSin = MathF.Abs(sin);

        // Вектор от центра OBB к центру AABB
        Vector2 centerDiff = aabbCenter - obb.Center;

        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        // --- ОСЬ 1: Глобальная ось X (AABB) ---
        // Проецируем OBB на ось X
        float obbProjX = obbHalf.X * absCos + obbHalf.Y * absSin;
        float overlapX = aabbHalf.X + obbProjX - MathF.Abs(centerDiff.X);
        if (overlapX <= 0) return false; // Пересечения нет

        minOverlap = overlapX;
        smallestAxis = Vector2.UnitX;

        // --- ОСЬ 2: Глобальная ось Y (AABB) ---
        // Проецируем OBB на ось Y
        float obbProjY = obbHalf.X * absSin + obbHalf.Y * absCos;
        float overlapY = aabbHalf.Y + obbProjY - MathF.Abs(centerDiff.Y);
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
        // Проецируем AABB на локальную ось X платформы
        float distObbX = MathF.Abs(centerDiff.X * cos + centerDiff.Y * sin); // Быстрый Dot Product
        float aabbProjObbX = aabbHalf.X * absCos + aabbHalf.Y * absSin;
        float overlapObbX = aabbProjObbX + obbHalf.X - distObbX;
        if (overlapObbX <= 0) return false;

        if (overlapObbX < minOverlap)
        {
            minOverlap = overlapObbX;
            smallestAxis = obbAxisX;
        }

        // --- ОСЬ 4: Локальная ось Y (OBB) ---
        // Проецируем AABB на локальную ось Y платформы
        float distObbY = MathF.Abs(centerDiff.X * -sin + centerDiff.Y * cos);
        float aabbProjObbY = aabbHalf.X * absSin + aabbHalf.Y * absCos;
        float overlapObbY = aabbProjObbY + obbHalf.Y - distObbY;
        if (overlapObbY <= 0) return false;

        if (overlapObbY < minOverlap)
        {
            minOverlap = overlapObbY;
            smallestAxis = obbAxisY;
        }

        // 4. Определение направления выталкивания
        // Нам нужно, чтобы вектор выталкивал AABB *от* OBB. 
        // Проверяем с помощью скалярного произведения (Dot product):
        if (Vector2.Dot(centerDiff, smallestAxis) < 0)
        {
            smallestAxis = -smallestAxis;
        }

        // Формируем итоговый вектор сдвига (MTV)
        mtv = smallestAxis * minOverlap;
        return true;
    }
}
