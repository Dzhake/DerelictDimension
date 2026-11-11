using DerelictDimension.ECS.Rewinding;
using Monod.ECS.Systems;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Drawing;

public class DrawSystem : SystemGroup
{
    public RenderTarget2D? MainRT;
    public static Vector2 Upscale = Vector2.One;

    public DrawSystem(string name = "DrawSystem") : base(name) { }


    public override void Update(SystemCtx ctx)
    {
        var renderSize = Renderer.RenderSize;
        LoadMissingAssets();
        Upscale = new(renderSize.X / TheGame.GameSize.X, renderSize.Y / TheGame.GameSize.Y);

        Renderer.SetRenderTarget(MainRT);
        Renderer.Clear(Color.Green);

        base.Update(ctx);
        ctx.cb.Playback();

        ctx.drawBatcher.Flush(ctx);

        DrawFinal();
    }

    private void DrawFinal()
    {
        Renderer.SetRenderTarget(null);
        var renderOffset = Renderer.RenderOffset;

        Renderer.Begin();
        Renderer.DrawTexture(MainRT!, renderOffset, Color.White);
        Renderer.End();

        Renderer.Begin();
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Time:{Time.TotalTimeSpan}\nCurrent Frame:{Rewind.CurrentFrame}\nEntities:{EcsMain.CurrentStore.Count}\nMousePos:{Input.MousePos()}", renderOffset, Color.White);
        Renderer.End();
    }

    private void LoadMissingAssets()
    {
        var renderSize = Renderer.RenderSize;

        if (MainRT is null || MainRT.Bounds.Size != renderSize)
        {
            MainRT?.Dispose();
            MainRT = new(Renderer.device, renderSize.X, renderSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
        }
    }
}
