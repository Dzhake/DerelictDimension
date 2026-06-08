using DerelictDimension.ECS.Physics;
using Microsoft.Xna.Framework;
using Monod.Graphics;

namespace DerelictDimension;

public static class RendererExt
{
    public static void DrawRotRect(RotatedRectangle rect, Color? color = null) => Renderer.DrawRotRect(rect.Center.X, rect.Center.Y, rect.Width, rect.Height, rect.Angle, color);
}
