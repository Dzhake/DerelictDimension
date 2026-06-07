using DerelictDimension.ECS.Battle;
using DerelictDimension.ECS.Physics;
using DerelictDimension.ECS.Rewinding;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Maths;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.Graphics.Extensions;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.TimeModule;
using System;

namespace DerelictDimension.ECS;

public class DrawSystem : BaseSystem
{
    private const float CardRadius = 0.3f;
    public Effect? CardModifyEffect;
    public Effect? InCardEffect;
    public RenderTarget2D? InCardRT;
    public RenderTarget2D? MainRT;
    public Texture2D Bg;
    public Texture2D CardBg;
    public Vector2 Upscale = Vector2.One;

    public ArchetypeQuery<ActorComponent> ActorsQuery;
    public ArchetypeQuery<SolidComponent> SolidsQuery;


    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        ActorsQuery = store.Query<ActorComponent>();
        SolidsQuery = store.Query<SolidComponent>();
    }

    protected override void OnUpdateGroup()
    {
        Point renderSize = Renderer.RenderSize;
        LoadMissingAssets();
        Upscale = new(renderSize.X / TheGame.GameSize.X, renderSize.Y / TheGame.GameSize.Y);
        UpdateShaders(renderSize, out float cardSide, out float lean);

        RenderTarget2D? currentRT = Renderer.RenderTarget;
        Renderer.SetRenderTarget(MainRT);
        Renderer.Clear(Renderer.EmptyColor);
        DrawGame();
        //DrawInCard(renderSize, cardSide, lean);
        Renderer.SetRenderTarget(currentRT);
        DrawFinal(renderSize, cardSide, lean);
    }

    private void DrawGame()
    {
        Renderer.Begin(samplerState: SamplerState.PointClamp);
        ActorsQuery.ForEachEntity(DrawActor);
        SolidsQuery.ForEachEntity(DrawSolid);
        Renderer.End();
    }

    private void DrawActor(ref ActorComponent actor, Entity entity)
    {
        var data = entity.Data;
        Vector2 pos;
        if (data.Has<Position2D>())
            pos = data.Get<Position2D>().Value;
        else
            pos = Vector2.Zero;
        bool isTimeless = data.Has<TimelessComponent>();

        RectangleF rect = actor.Hitbox;
        rect.Location += pos;
        rect.X *= Upscale.X;
        rect.Y *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;
        Color color = Color.Lerp(Color.Yellow, Color.LightGray, Rewind.Active ? 0.8f : 0);
        if (isTimeless) color = Color.Lime;
        Renderer.DrawRect((Rectangle)rect, color);
        Renderer.DrawLine(rect.Center, rect.Center + (actor.Velocity * Time.DeltaTime), Color.Red, 5);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"{actor.Velocity.X}\n{actor.Velocity.Y}\n{actor.RidingEntityId}", rect.Location, Color.Black);
    }

    private void DrawSolid(ref SolidComponent solid, Entity entity)
    {
        var data = entity.Data;
        Vector2 pos;
        if (data.Has<Position2D>())
            pos = data.Get<Position2D>().Value;
        else
            pos = Vector2.Zero;
        RectangleF rect = solid.Hitbox;
        rect.Location += pos;
        rect.X *= Upscale.X;
        rect.Y *= Upscale.Y;
        rect.Width *= Upscale.X;
        rect.Height *= Upscale.Y;

        Color color = Color.Lerp(Color.Blue, Color.LightGray, Rewind.Active ? 0.8f : 0);
        Renderer.DrawRect((Rectangle)rect, color);
        bool dynamic = data.Has<MovingSolidComponent>();
        if (dynamic)
        {
            ref var dyn = ref data.Get<MovingSolidComponent>();
            GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"{dyn.Velocity.X}\n{dyn.Velocity.Y}", rect.Location, Color.White);
        }
    }

    private void DrawInCard(Point renderSize, float cardSide, float lean)
    {
        if (lean != 0)
        {
            Renderer.SetRenderTarget(InCardRT);
            Renderer.Clear(Renderer.EmptyColor);

            Renderer.Begin(samplerState: SamplerState.PointClamp);
            Renderer.DrawRect(new Rectangle((int)(renderSize.X - cardSide) / 2, (int)(renderSize.Y - cardSide) / 2, (int)cardSide, (int)(cardSide * Math.Abs(lean) / 2)), Color.Black.WithAlpha(120));
            Renderer.End();

            Renderer.SetRenderTarget(MainRT);
            Renderer.Begin(samplerState: SamplerState.PointClamp, effect: InCardEffect);
            Renderer.DrawTexture(InCardRT!, Vector2.Zero, Color.White);
            Renderer.End();
        }
    }

    private void DrawFinal(Point renderSize, float cardSide, float lean)
    {
        Vector2 renderOffset = Renderer.RenderOffset;
        /*Renderer.Begin(samplerState: SamplerState.PointClamp);
        Renderer.DrawTexture(Bg, renderOffset, scale: Upscale);
        Renderer.End();*/

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        Renderer.DrawTexture(MainRT!, renderOffset, Color.White);
        Renderer.End();

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Lean: {lean}\nTarget:{UpdateCardSystem.Target}\nWindowSize:{Renderer.WindowSizeP}\nRenderSize:{Renderer.RenderSize}\nRenderOffset:{Renderer.RenderOffset}\nCardSide:{cardSide}\nTime:{Time.TotalTimeSpan}\nRewinding:{Rewind.Active}\nCurrentIndex:{RewindPostUpdateSystem.CurrentIndex}", renderOffset, Color.White);
        Renderer.End();
    }

    private void UpdateShaders(Point renderSize, out float cardSide, out float lean)
    {
        cardSide = 320 * Upscale.X;
        lean = UpdateCardSystem.GetActualLean();
        float halfSideX = cardSide / renderSize.X / 2;
        float halfSideY = cardSide / renderSize.Y / 2;

        CardModifyEffect!.Parameters["HalfSideX"]?.SetValue(halfSideX);
        CardModifyEffect.Parameters["HalfSideY"]?.SetValue(halfSideY);
        CardModifyEffect.Parameters["Lean"]?.SetValue(lean);
        CardModifyEffect.Parameters["CardRadius"]?.SetValue(CardRadius);
        CardModifyEffect.Parameters["TotalTime"]?.SetValue(Time.TotalTime);

        InCardEffect!.Parameters["HalfSideX"]?.SetValue(halfSideX);
        InCardEffect.Parameters["HalfSideY"]?.SetValue(halfSideY);
        InCardEffect.Parameters["CardRadius"]?.SetValue(CardRadius);
    }

    private void DrawFighter(ref FighterComponent fighter, ref Sprite2D sprite, Entity entity)
    {
        var data = entity.Data;
        if (sprite.Texture is null) return;
        GetDrawInfo(sprite, data, out var _, out var scale, out var depth, out var rotation, out var origin);
        scale ??= Vector2.One;

        Vector2 pos = UpdateBattleSystem.GridPosToWorldPos(fighter.Position);
        SpriteEffects spriteEffects = sprite.spriteEffects;
        if (fighter.LooksLeft) spriteEffects |= SpriteEffects.FlipHorizontally;
        Renderer.DrawTexture(sprite.Texture, pos * Upscale, sprite.color, null, rotation ?? 0f, origin, scale * Upscale, spriteEffects, depth ?? 0f);
    }

    private void DrawGameLayerSprite(ref Sprite2D sprite, Entity entity)
    {
        var data = entity.Data;
        if (sprite.Texture is null) return;
        GetDrawInfo(sprite, data, out var pos, out var scale, out var depth, out var rotation, out var origin);
        scale ??= Vector2.One;

        Vector2 mousePos = Input.MousePos();
        Vector2 realPosition = pos * Upscale ?? new(mousePos.X, mousePos.Y);
        Renderer.DrawTexture(sprite.Texture, realPosition, sprite.color, null, rotation ?? 0, origin, scale * Upscale, sprite.spriteEffects, depth ?? 0);
    }

    private static void GetDrawInfo(Sprite2D sprite, EntityData data, out Vector2? pos, out Vector2? scale, out float? depth, out float? rotation, out Vector2? origin)
    {
        if (data.TryGet<Position2D>(out var position2D))
            pos = position2D.Value;
        else
            pos = null;

        if (data.TryGet<Scale2D>(out var scale2D))
            scale = scale2D.Value;
        else
            scale = null;

        if (data.TryGet<RenderDepth>(out var renderDepth))
            depth = renderDepth.Depth;
        else
            depth = null;

        if (data.TryGet<Rotation2D>(out var rotation2d))
            rotation = rotation2d.Angle;
        else
            rotation = null;

        if (sprite.Texture is null) origin = null;
        else origin = new(sprite.Texture.Width / 2f, sprite.Texture.Height / 2f);
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

        if (CardModifyEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            CardModifyEffect = Assets.Get<Effect>("Effects/CardModify.mgfx");

        if (InCardEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            InCardEffect = Assets.Get<Effect>("Effects/InCard.mgfx");

        if (Bg?.IsDisposed != false || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Sprites/Bg.png");
    }
}
