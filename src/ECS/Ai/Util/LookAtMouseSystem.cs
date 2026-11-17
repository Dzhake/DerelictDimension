using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.InputModule;
using Monod.MathModule;
using Monod.Utils.Extensions;
using SDL3;
using System;

namespace DerelictDimension.ECS.Ai.Visual;

public class LookAtMouseSystem : SystemBase
{
    public static bool RelativeRotation = false;

    public override void Update(SystemCtx ctx)
    {
        SDL.SetWindowRelativeMouseMode(Renderer.Window.Handle, RelativeRotation);
        Vector2 mousePos;
        if (RelativeRotation)
        {
            SDL.GetRelativeMouseState(out mousePos.X, out mousePos.Y);
        }
        else
        {
            mousePos = Coords.ScreenToWorld(Input.MousePos());
        }
        foreach (var entity in ctx.GetQuery(Queries.LookAtMouseTag).Entities)
        {
            ref var transform = ref entity.Data.Ensure<Transform2D>(ctx.cb);
            float rotation;
            if (RelativeRotation)
            {
                float rotspeed = 0.003f; //TODO (QOL:low) make this a setting 'mouse sensitivity'
                rotation = (transform.Rotation + (mousePos.X * rotspeed)) % MathF.Tau;
            }
            else
            {
                var mouseOffset = mousePos - entity.GetWorldPos();
                rotation = mouseOffset.ToAngleM();
            }

            if (MathF.Abs(rotation - transform.Rotation) > MathM.Epsilon)
            {
                transform.Rotation = rotation;
                entity.SetTransformDirtyRecursive();
            }
        }
    }

}
