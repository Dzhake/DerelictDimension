using DerelictDimension.ECS.Ai;
using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Physics.Components;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS.Systems;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.Fonts;
using Monod.MathModule;
using Monod.TimeModule;
using Monod.Utils.Extensions;

namespace DerelictDimension.ECS.Drawing;

public class DrawSystem : BaseSystem
{
    public Effect? RewindEffect;
    public RenderTarget2D? InCardRT;
    public RenderTarget2D? MainRT;
    public Texture2D Bg;
    public Texture2D CardBg;
    public static Vector2 Upscale = Vector2.One;

    public ArchetypeQuery<MobileComponent> MobilesQuery;
    public ArchetypeQuery<SupportComponent> SupportsQuery;
    public ArchetypeQuery<Transform2D, HitboxComponent> HitboxesQuery;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        MobilesQuery = store.Query<MobileComponent>();
        SupportsQuery = store.Query<SupportComponent>();
        HitboxesQuery = store.Query<Transform2D, HitboxComponent>();
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
        //SupportsQuery.ForEachEntity(DrawSupport);
        //MobilesQuery.ForEachEntity(DrawMobile);
        bool drawHitboxes = true;
        if (drawHitboxes)
        {
            Renderer.Begin(effect: RewindEffect);
            HitboxesQuery.ForEachEntity(DrawHitbox);
            Renderer.End();
        }
    }

    private void DrawHitbox(ref Transform2D transform, ref HitboxComponent hitboxC, Entity entity)
    {
        var hitbox = PhysicsSystem.GetWorldHitbox(ref hitboxC, ref transform);
        var data = entity.Data;

        bool isLethal = data.Has<LethalComponent>();
        bool isSolid = data.Has<SolidComponent>();
        bool isSupport = data.Has<SupportComponent>();
        bool isMobile = data.Has<MobileComponent>();
        bool isBouncy = data.Has<BouncyComponent>();

        float alpha = hitboxC.Collidable ? 1 : 0.5f;
        int lineWidth = 2;

        Rectangle rect = new(hitbox.TopLeft.ToPoint(), hitbox.Size.ToPoint());

        if (isLethal)
        {
            Renderer.DrawRect(rect, Color.Red * alpha);
        }

        Color mainColor = Color.Blue;
        if (isMobile)
        {
            mainColor = (isSupport || isSolid) ? new Color(255, 250, 205) : Color.Yellow;
        }
        mainColor *= alpha;

        if (isSolid)
        {
            Renderer.DrawRect(rect, mainColor);
        }
        else if (isSupport)
        {
            Direction4 normals = data.Get<SupportComponent>().Normals;

            if ((normals & Direction4.Up) != 0)
            {
                Renderer.DrawLine(hitbox.TopLeft, hitbox.TopRight, mainColor, lineWidth);
                Renderer.DrawLine(hitbox.TopLeft + new Vector2(0, lineWidth), hitbox.TopRight + new Vector2(0, lineWidth), mainColor * 0.7f, lineWidth);
            }
            if ((normals & Direction4.Down) != 0) Renderer.DrawLine(hitbox.BottomLeft, hitbox.BottomRight, mainColor, lineWidth);
            if ((normals & Direction4.Left) != 0) Renderer.DrawLine(hitbox.TopLeft, hitbox.BottomLeft, mainColor, lineWidth);
            if ((normals & Direction4.Right) != 0) Renderer.DrawLine(hitbox.TopRight, hitbox.BottomRight, mainColor, lineWidth);
        }
        else if (!isLethal || isMobile)
        {
            Renderer.DrawHollowRect(rect, mainColor, lineWidth);
        }

        if (isBouncy)
        {
            Renderer.DrawLine(hitbox.TopLeft, hitbox.TopRight, new Color(136, 14, 201) * alpha, lineWidth + 1);
        }
    }

    private void DrawMobile(ref MobileComponent mobile, Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<HitboxComponent>() || !data.Has<Transform2D>()) return;
        bool isTimeless = data.Has<TimelessComponent>();
        ref var transform = ref data.Get<Transform2D>();
        bool canJump = false;

        if (data.Has<PlayerAi>() && data.Get<PlayerAi>().CoyoteTimeLeft >= 0) canJump = true;

        ref var hitbox = ref data.Get<HitboxComponent>();
        AABB rect = PhysicsSystem.GetWorldHitbox(ref hitbox, ref transform);

        rect.CenterX *= Upscale.X;
        rect.CenterY *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;
        Color color = new(176 / 255f, 176 / 255f, 39 / 255f);
        if (isTimeless) color = Color.Lime;
        if (canJump) color = Color.Orange;
        if (!hitbox.Collidable) color.A /= 2;
        Renderer.Begin(effect: isTimeless || !Rewind.Active ? null : RewindEffect);
        RendererExt.DrawRotRect(rect, transform.GetFlippedRotation(), color);
        Renderer.DrawLine(rect.Center, rect.Center + (mobile.Velocity * Time.DeltaTime), Color.Red, 5);
        //Renderer.DrawLine(new(0, mobile.HighestPoint), new(Renderer.WindowSize.X, mobile.HighestPoint), Color.LightBlue, 1);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"{mobile.Velocity.X}\n{mobile.Velocity.Y}\n{mobile.SupportingEntityPid}", rect.Center, Color.White);
        Renderer.End();
    }

    private void DrawSupport(ref SupportComponent support, Entity entity)
    {
        var data = entity.Data;
        if (!data.Has<HitboxComponent>() || !data.Has<Transform2D>()) return;
        ref var transform = ref data.Get<Transform2D>();
        ref var hitbox = ref data.Get<HitboxComponent>();
        bool isSolid = data.Has<SolidComponent>();

        AABB rect = PhysicsSystem.GetWorldHitbox(ref hitbox, ref transform);

        rect.CenterX *= Upscale.X;
        rect.CenterY *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;

        Color color = new(48 / 255f, 37 / 255f, 138 / 255f);
        if (!isSolid) color = Color.Purple;
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

        if (RewindEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            RewindEffect = Assets.Get<Effect>("Effects/Rewind.fx");

        if (Bg?.IsDisposed != false || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Sprites/Bg.png");
    }
}
