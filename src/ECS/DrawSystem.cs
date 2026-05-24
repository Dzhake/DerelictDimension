using DerelictDimension.ECS.Battle;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.Graphics.Extensions;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using System;

namespace DerelictDimension.ECS;

public class DrawSystem : BaseSystem
{
    private const float CardRadius = 0.25f;
    public Effect? CardModifyEffect;
    public Effect? InCardEffect;
    public RenderTarget2D? GameRT;
    public RenderTarget2D? InCardRT;
    public RenderTarget2D? PostCardRT;
    public Texture2D Bg;
    public Texture2D CardBg;

    public ArchetypeQuery<Sprite2D> GameLayerQuery;
    public ArchetypeQuery<FighterComponent, Sprite2D> FightersQuery;


    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        GameLayerQuery = store.Query<Sprite2D>().AllTags(Tags.Get<GameLayerTag>());
        FightersQuery = store.Query<FighterComponent, Sprite2D>();
    }

    private void VerifyOrGetAssets(Point windowSizeP)
    {
        if (GameRT is null)
        {
            GameRT?.Dispose();
            GameRT = new(Renderer.device, (int)TheGame.GameSize.X, (int)TheGame.GameSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        if (InCardRT is null || InCardRT.Bounds.Size != windowSizeP)
        {
            InCardRT?.Dispose();
            InCardRT = new(Renderer.device, windowSizeP.X, windowSizeP.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        if (PostCardRT is null || PostCardRT.Bounds.Size != windowSizeP)
        {
            PostCardRT?.Dispose();
            PostCardRT = new(Renderer.device, windowSizeP.X, windowSizeP.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        if (CardModifyEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            CardModifyEffect = Assets.Get<Effect>("Effects/CardModify.mgfx");

        if (InCardEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            InCardEffect = Assets.Get<Effect>("Effects/InCard.mgfx");

        if (Bg?.IsDisposed != false || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Bg.png");
    }

    protected override void OnUpdateGroup()
    {
        Point windowSizeP = Renderer.WindowSizeP;
        VerifyOrGetAssets(windowSizeP);

        RenderTarget2D? currentRT = Renderer.RenderTarget;

        Renderer.SetRenderTarget(GameRT);
        Renderer.Clear(Renderer.EmptyColor);

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        GameLayerQuery.ForEachEntity(DrawGameLayerSprite);
        FightersQuery.ForEachEntity(DrawFighter);
        Renderer.End();

        Renderer.SetRenderTarget(PostCardRT);
        Renderer.Clear(Renderer.EmptyColor);

        Vector2 upscale = new(windowSizeP.X / TheGame.GameSize.X);
        float cardSide = 128 * upscale.X;
        float lean = UpdateLeanSystem.GetActualLean();
        float halfSideX = cardSide / windowSizeP.X / 2;
        CardModifyEffect!.Parameters["HalfSideX"].SetValue(halfSideX);
        float halfSideY = cardSide / windowSizeP.Y / 2;
        CardModifyEffect.Parameters["HalfSideY"].SetValue(halfSideY);
        CardModifyEffect.Parameters["Lean"].SetValue(lean);
        CardModifyEffect.Parameters["CardRadius"].SetValue(CardRadius);
        Renderer.Begin(samplerState: SamplerState.PointClamp);
        Renderer.DrawTexture(GameRT!, Vector2.Zero, scale: upscale);
        Renderer.End();

        if (lean != 0)
        {
            Renderer.SetRenderTarget(InCardRT);
            Renderer.Clear(Renderer.EmptyColor);
            InCardEffect!.Parameters["HalfSideX"].SetValue(halfSideX);
            InCardEffect.Parameters["HalfSideY"].SetValue(halfSideY);
            InCardEffect.Parameters["CardRadius"].SetValue(CardRadius);
            Renderer.Begin(samplerState: SamplerState.PointClamp);
            Renderer.DrawRect(new Rectangle((int)(windowSizeP.X - cardSide) / 2, (int)(windowSizeP.Y - cardSide) / 2, (int)cardSide, (int)(cardSide * Math.Abs(lean) / 2)), Color.Black.WithAlpha(120));
            Renderer.End();

            Renderer.SetRenderTarget(PostCardRT);
            Renderer.Begin(samplerState: SamplerState.PointClamp, effect: InCardEffect);
            Renderer.DrawTexture(InCardRT!, Vector2.Zero, Color.White);
            Renderer.End();
        }


        Renderer.SetRenderTarget(currentRT);

        /*Renderer.Begin(samplerState: SamplerState.PointClamp);
        Renderer.DrawTexture(Bg, Vector2.Zero, Color.White);
        Renderer.End();*/

        Renderer.Begin(samplerState: SamplerState.PointClamp, effect: CardModifyEffect);
        Renderer.DrawTexture(PostCardRT!, Vector2.Zero, Color.White);
        Renderer.End();

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Lean: {lean}\nTarget:{UpdateLeanSystem.Target}\nScreenWidth:{windowSizeP.X}\nScreenHeight:{windowSizeP.Y}\nCardSide:{cardSide}", Vector2.Zero, Color.White);
        Renderer.End();
    }

    private void DrawFighter(ref FighterComponent fighter, ref Sprite2D sprite, Entity entity)
    {
        var data = entity.Data;
        if (sprite.Texture is null) return;
        GetDrawInfo(sprite, data, out var _, out var scale, out var depth, out var rotation, out var origin);

        Vector2 pos = UpdateBattleSystem.GridPosToWorldPos(fighter.Position);
        SpriteEffects spriteEffects = sprite.spriteEffects;
        if (fighter.LooksLeft) spriteEffects |= SpriteEffects.FlipHorizontally;
        Renderer.DrawTexture(sprite.Texture, pos, sprite.color, null, rotation ?? 0f, origin, scale, spriteEffects, depth ?? 0f);
    }

    private void DrawGameLayerSprite(ref Sprite2D sprite, Entity entity)
    {
        var data = entity.Data;
        if (sprite.Texture is null) return;
        GetDrawInfo(sprite, data, out var pos, out var scale, out var depth, out var rotation, out var origin);

        Vector2 mousePos = Input.MousePos();
        Vector2 windowSize = Renderer.WindowSize;
        Vector2 gameSize = TheGame.GameSize;
        Vector2 p = pos ?? new(mousePos.X / windowSize.X * gameSize.X, mousePos.Y / windowSize.Y * gameSize.Y);
        Renderer.DrawTexture(sprite.Texture, p, sprite.color, null, rotation ?? 0, origin, scale, sprite.spriteEffects, depth ?? 0);
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
}
