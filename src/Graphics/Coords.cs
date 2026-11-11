using DerelictDimension.ECS.PhysicsModule;

namespace Monod.Graphics;

/// <summary>
/// Helper class for converting coordinates from one space to another.
/// </summary>
public static class Coords
{
    public static Vector2 ScreenToWorld(Vector2 screen)
    {
        return (screen - Renderer.RenderOffset) / Physics.PixelsInMeter;
    }
}
