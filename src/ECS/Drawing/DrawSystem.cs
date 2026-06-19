using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing;

public class DrawSystem : BaseSystem
{
    private const float CardRadius = 0.3f;
    public Effect? RewindEffect;
    public Effect? InCardEffect;
    public RenderTarget2D? InCardRT;
    public RenderTarget2D? MainRT;
    public Texture2D Bg;
    public Texture2D CardBg;
    public static Vector2 Upscale = Vector2.One;

    public ArchetypeQuery<MobileComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;


    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent>();
        SupportsQuery = store.Query<SupportComponent>();
    }

    protected override void OnUpdateGroup()
    {
        Point renderSize = Renderer.RenderSize;
        LoadMissingAssets();
        Upscale = new(renderSize.X / TheGame.GameSize.X, renderSize.Y / TheGame.GameSize.Y);
        UpdateShaders(renderSize, out float cardSide);

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
        MobilesQuery.ForEachEntity(DrawMobile);
        SupportsQuery.ForEachEntity(DrawSupport);
    }

    private void DrawMobile(ref MobileComponent mobile, Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<HitboxComponent>() || !data.Has<Transform2D>()) return;
        bool isTimeless = data.Has<TimelessComponent>();
        ref var transform = ref data.Get<Transform2D>();

        AABB rect = PhysicsSystem.GetWorldHitbox(ref data.Get<HitboxComponent>(), ref transform);

        rect.CenterX *= Upscale.X;
        rect.CenterY *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;
        Color color = new(176 / 255f, 176 / 255f, 39 / 255f);
        if (isTimeless) color = Color.Lime;
        Renderer.Begin(effect: isTimeless || !Rewind.Active ? null : RewindEffect);
        RendererExt.DrawRotRect(rect, transform.GetFlippedRotation(), color);
        Renderer.DrawLine(rect.Center, rect.Center + (mobile.Velocity * Time.DeltaTime), Color.Red, 5);
        //Renderer.DrawLine(new(0, mobile.HighestPoint), new(Renderer.WindowSize.X, mobile.HighestPoint), Color.LightBlue, 1);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"{mobile.Velocity.X}\n{mobile.Velocity.Y}\n{mobile.SupportingEntityId}", rect.Center, Color.Black);
        Renderer.End();
    }

    private void DrawSupport(ref SupportComponent support, Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<HitboxComponent>() || !data.Has<Transform2D>()) return;
        ref var transform = ref data.Get<Transform2D>();
        AABB rect = PhysicsSystem.GetWorldHitbox(ref data.Get<HitboxComponent>(), ref transform);
        rect.CenterX *= Upscale.X;
        rect.CenterY *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;

        Color color = new(48 / 255f, 37 / 255f, 138 / 255f);
        if (support.MakeTimeless && Rewind.CurrentFrame % 60 < 20) color.AddRgb(25);
        Renderer.Begin(effect: Rewind.Active ? RewindEffect : null);
        RendererExt.DrawRotRect(rect, transform.GetFlippedRotation(), color);
        Renderer.End();
    }

    private void DrawFinal()
    {
        Vector2 renderOffset = Renderer.RenderOffset;
        /*Renderer.Begin();
        Renderer.DrawTexture(Bg, renderOffset, scale: Upscale);
        Renderer.End();*/

        Renderer.Begin();
        Renderer.DrawTexture(MainRT!, renderOffset, Color.White);
        Renderer.End();

        Renderer.Begin();
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Time:{Time.TotalTimeSpan}\nCurrent Frame:{Rewind.CurrentFrame}\nRewinding:{Rewind.Active}\nRewind Speed:{Rewind.RewindSpeed}\nLast Valid Frame: {RewindPostUpdateSystem.LastValidFrame}", renderOffset, Color.White);
        Renderer.End();
    }

    private void UpdateShaders(Point renderSize, out float cardSide)
    {
        cardSide = 320 * Upscale.X;
        float halfSideX = cardSide / renderSize.X / 2;
        float halfSideY = cardSide / renderSize.Y / 2;

        RewindEffect!.Parameters["SaturationChange"]?.SetValue(Rewind.GetSaturationChange());

        InCardEffect!.Parameters["HalfSideX"]?.SetValue(halfSideX);
        InCardEffect.Parameters["HalfSideY"]?.SetValue(halfSideY);
        InCardEffect.Parameters["CardRadius"]?.SetValue(CardRadius);
    }

    private void LoadMissingAssets()
    {
        Point renderSize = Renderer.RenderSize;
        if (InCardRT is null || InCardRT.Bounds.Size != renderSize)
        {
            InCardRT?.Dispose();
            InCardRT = new(Renderer.device, renderSize.X, renderSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
        }

        if (MainRT is null || MainRT.Bounds.Size != renderSize)
        {
            MainRT?.Dispose();
            MainRT = new(Renderer.device, renderSize.X, renderSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
        }

        if (RewindEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            RewindEffect = Assets.Get<Effect>("Effects/Rewind.mgfx");

        if (InCardEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            InCardEffect = Assets.Get<Effect>("Effects/InCard.mgfx");

        if (Bg?.IsDisposed != false || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Sprites/Bg.png");
    }
}
