using DerelictDimension.ECS.PhysicsModule;
using Monod.ECS.DefaultComponents;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.InputModule;
using Monod.MathModule;
using Monod.Utils.Extensions;
using MonoGame.Extended;
using SDL3;
using System;

namespace DerelictDimension.ECS.Ai.Visual;

public class LookAtMouseSystem : SystemBase
{
    public static bool RelativeRotation = false;

    public override void Update(SystemCtx ctx)
    {
        SDL.SDL_SetWindowRelativeMouseMode(Renderer.Window.Handle, RelativeRotation);
        foreach (var entity in ctx.GetQuery(Queries.LookAtMouseTag).Entities)
        {
            ref var transform = ref entity.Data.Ensure<Transform2D>(ctx.cb);
            float rotation;
            if (RelativeRotation)
            {
                SDL.SDL_GetRelativeMouseState(out float x, out float y);
                var mouseOffset = new Vector2(x, y);
                float rotspeed = 0.003f; //TODO (QOL:low) make this a setting 'mouse sensitivity'
                rotation = (transform.Rotation + (mouseOffset.X * rotspeed)) % MathF.Tau;
            }
            else
            {
                var mouseOffset = Input.MousePos() - (entity.GetWorldPos() * Physics.PixelsInMeter);
                rotation = mouseOffset.ToAngle();
            }

            if (MathF.Abs(rotation - transform.Rotation) > MathM.Epsilon)
            {
                transform.Rotation = rotation;
                entity.SetTransformDirtyRecursive();
            }
        }
    }

}
