using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.TimeModule;

namespace DerelictDimension.ECS.Drawing;

public class DrawSystem : BaseSystem
{
    public Effect? RewindEffect;
    public RenderTarget2D? InCardRT;
    public RenderTarget2D? MainRT;
    public Texture2D Bg;
    public Texture2D CardBg;
    public static Vector2 Upscale = Vector2.One;


    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
    }

    protected override void OnUpdateGroup()
    {
        Point renderSize = Renderer.RenderSize;
        LoadMissingAssets();
        Upscale = new(renderSize.X / TheGame.GameSize.X, renderSize.Y / TheGame.GameSize.Y);
        UpdateShaders(renderSize);

        RenderTarget2D? currentRT = Renderer.RenderTarget;
        Renderer.SetRenderTarget(MainRT);
        Renderer.Clear(Renderer.EmptyColor);
        DrawGame();
        //DrawInCard(renderSize, cardSide, lean);
        Renderer.SetRenderTarget(currentRT);
        DrawFinal();
    }

    private void DrawGame()
    {
    }

    private void DrawFinal()
    {
        Vector2 renderOffset = Renderer.RenderOffset;

        Renderer.Begin();
        Renderer.DrawTexture(MainRT!, renderOffset, Color.White);
        Renderer.End();

        Renderer.Begin();
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Time:{Time.TotalTimeSpan}\nCurrent Frame:{Rewind.CurrentFrame}\nRewinding:{Rewind.Active}\nRewind Speed:{Rewind.RewindSpeed}\nLast Valid Frame: {RewindPostUpdateSystem.LastValidFrame}\nEntities:{TheGame.Store.Count}", renderOffset, Color.White);
        Renderer.End();
    }

    private void UpdateShaders(Point renderSize)
    {
        RewindEffect!.Parameters["SaturationChange"]?.SetValue(Rewind.GetSaturationChange());
    }

    private void LoadMissingAssets()
    {
        Point renderSize = Renderer.RenderSize;

        if (MainRT is null || MainRT.Bounds.Size != renderSize)
        {
            MainRT?.Dispose();
            MainRT = new(Renderer.device, renderSize.X, renderSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
        }

        if (RewindEffect?.IsDisposed is null or true || Assets.ReloadThisFrame)
            RewindEffect = Assets.Get<Effect>("Effects/Rewind.fx");

        if (Bg?.IsDisposed is null or true || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Sprites/Bg.png");
    }
}
