using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsModule;
using Monod.ECS.DefaultComponents;
using Monod.Graphics;
using Monod.Graphics.ECS.Sprite;
using Monod.Graphics.Fonts;
using Monod.InputModule;

namespace DerelictDimension.ECS;

public class DrawSystem : BaseSystem
{
    public Effect? CardEffect;
    public RenderTarget2D ScreenRT;
    public RenderTarget2D PostCardRT;
    public Texture2D Bg;
    public ArchetypeQuery<Sprite2D> GameLayerQuery;

    protected override void OnAddStore(EntityStore store)
    {
        base.OnAddStore(store);
        GameLayerQuery = store.Query<Sprite2D>().AllTags(Tags.Get<GameLayerTag>());
    }

    protected override void OnUpdateGroup()
    {
        Rectangle clientBounds = Renderer.Window.ClientBounds;
        if (ScreenRT is null)
        {
            ScreenRT?.Dispose();
            ScreenRT = new(Renderer.device, (int)TheGame.GameSize.X, (int)TheGame.GameSize.Y, false, SurfaceFormat.Color, DepthFormat.None, 2, RenderTargetUsage.PreserveContents);
        }

        if (PostCardRT is null || PostCardRT.Bounds.Size != clientBounds.Size)
        {
            PostCardRT?.Dispose();
            PostCardRT = new(Renderer.device, clientBounds.Width, clientBounds.Height, false, SurfaceFormat.Color, DepthFormat.None, 2, RenderTargetUsage.PreserveContents);
        }

        if (CardEffect?.IsDisposed != false || Assets.ReloadThisFrame)
            CardEffect = Assets.Get<Effect>("Effects/Card.mgfx");

        if (Bg?.IsDisposed != false || Assets.ReloadThisFrame)
            Bg = Assets.Get<Texture2D>("Bg.png");

        RenderTarget2D? currentRT = Renderer.RenderTarget;
        Renderer.SetRenderTarget(ScreenRT);
        Renderer.Clear(new(0, 0, 0, 0));

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        GameLayerQuery.ForEachEntity(DrawShip);
        Renderer.End();

        Vector2 upscale = new(clientBounds.Width / 320f);
        Renderer.SetRenderTarget(PostCardRT);
        Renderer.Clear(new(0, 0, 0, 0));

        float cardSide = 64 * upscale.X;
        float lean = UpdateLeanSystem.GetActualLean();
        CardEffect.Parameters["HalfSideX"].SetValue(cardSide / clientBounds.Width / 2);
        CardEffect.Parameters["HalfSideY"].SetValue(cardSide / clientBounds.Height / 2);
        CardEffect.Parameters["Lean"].SetValue(lean);
        CardEffect.Parameters["CardRadius"].SetValue(0.25f);
        Renderer.Begin(samplerState: SamplerState.PointClamp, effect: CardEffect);
        Renderer.DrawTexture(ScreenRT, Vector2.Zero, scale: upscale);
        Renderer.End();

        Renderer.SetRenderTarget(currentRT);

        Renderer.Begin(samplerState: SamplerState.PointClamp);
        Renderer.DrawTexture(Bg, Vector2.Zero, Color.White);
        Renderer.DrawTexture(PostCardRT, Vector2.Zero, Color.White);
        GlobalFonts.MenuFont.DrawString(Renderer.spriteBatch, $"Lean: {lean}\nTarget:{UpdateLeanSystem.Target}\nScreenWidth:{clientBounds.Width}\nScreenHeight:{clientBounds.Height}\nCardSide:{cardSide}", Vector2.Zero, Color.White);
        Renderer.End();
    }

    private void DrawShip(ref Sprite2D sprite, Entity entity)
    {
        var data = entity.Data;
        if (sprite.Texture is null) return;

        Vector2 scale;
        if (data.TryGet<Scale2D>(out var scale2D))
            scale = scale2D.Value;
        else
            scale = Vector2.One;

        float depth;
        if (data.TryGet<RenderDepth>(out var renderDepth))
            depth = renderDepth.Depth;
        else
            depth = 0;

        float rotation;
        if (data.TryGet<Rotation2D>(out var rotation2d))
            rotation = rotation2d.Angle;
        else
            rotation = 0;

        Vector2 origin = new(sprite.Texture.Width / 2f, sprite.Texture.Height / 2f);
        float lean = UpdateLeanSystem.GetActualLean();

        Vector2 mousePos = Input.MousePos();
        Vector2 windowSize = Renderer.Window.ClientBounds.Size.ToVector2();
        Vector2 gameSize = TheGame.GameSize;
        Vector2 pos = new(mousePos.X / windowSize.X * gameSize.X, mousePos.Y / windowSize.Y * gameSize.Y);
        Renderer.DrawTexture(sprite.Texture, pos, null, sprite.color, rotation, origin, scale, SpriteEffects.None, depth);
    }
}
